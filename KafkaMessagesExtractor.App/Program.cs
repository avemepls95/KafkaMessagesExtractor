﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KafkaMessagesExtractor.App.Constants;
using KafkaMessagesExtractor.DomainModel;
using Newtonsoft.Json;
using Serilog;

namespace KafkaMessagesExtractor.App
{
    class Program
    {
        static async Task Main(string[] args)
        {
            SetupApplication();

            var grabMessagesCount = GetGrabMessagesCount();
            Console.WriteLine($"Шаг: {grabMessagesCount} сообщений");

            var (host, topicName, searchValue) = GetInputData();

            var kafkaClient = new KafkaClient(Log.Logger);
            var messagesMetaDataItems = await kafkaClient.GetMessagesMetaData(host, topicName, CancellationToken.None);
            var topicsSearchInfos = messagesMetaDataItems.Select(i => new TopicSearchInfo
            {
                Partition = i.Partition,
                CurrentLastOffset = i.LastOffset - grabMessagesCount
            }).ToList();

            var scannedMessagesCount = 0;
            while (true)
            {
                Console.WriteLine("Поиск...");

                var lastMessagesFromPartitions = new List<DomainItem>();
                var messagesFromAllPartitions = new List<DomainItem>();
                foreach (var topicsSearchInfo in topicsSearchInfos)
                {
                    var searchParameters = new SearchParameters
                    {
                        Url = host,
                        TopicName = topicName,
                        Partition = topicsSearchInfo.Partition,
                        Offset = topicsSearchInfo.CurrentLastOffset,
                        Count = grabMessagesCount
                    };

                    var outputMessagesContainer = new OutputMessagesContainer();
                    var partitionMessages = await kafkaClient.FindAsync(
                        searchParameters,
                        outputMessagesContainer,
                        CancellationToken.None);

                    if (outputMessagesContainer.Any())
                    {
                        foreach (var message in outputMessagesContainer)
                        {
                            Console.WriteLine(message);
                        }
                    }

                    messagesFromAllPartitions.AddRange(partitionMessages);
                    lastMessagesFromPartitions.Add(partitionMessages.OrderBy(m => m.Timestamp).First());
                }

                var result = messagesFromAllPartitions
                    .Where(m => JsonConvert.SerializeObject(m.Message.Data).Contains(searchValue))
                    .ToArray();

                scannedMessagesCount += grabMessagesCount;
                if (!result.Any())
                {
                    Console.WriteLine($"Нет результатов.");
                }
                else
                {
                    Console.WriteLine("Результат:");
                    foreach (var domainItem in result)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(domainItem) + Environment.NewLine);
                    }
                }

                var lastMessage = lastMessagesFromPartitions.OrderBy(p => p.Timestamp).Last();
                Console.WriteLine($"Всего просканировано {scannedMessagesCount} cообщений. Дата самого раннего: {lastMessage.Timestamp}");
                Console.WriteLine($"Желаете продолжить поиск? Нажмите y, если да. Иначе любую другую клавишу");
                var response = Console.ReadKey();
                Console.WriteLine();
                if (response.KeyChar != 'y')
                {
                    Console.WriteLine("Поиск завершен.");
                    return;
                }

                foreach (var topicsSearchInfo in topicsSearchInfos)
                {
                    topicsSearchInfo.CurrentLastOffset -= grabMessagesCount;
                }
            }
        }

        static (string host, string topicName, string searchValue) GetInputData()
        {
            KafkaTopic selectedTopic = null;
            string searchValue;

            int hostSelectedIndex = 0, topicNameSelectedIndex = 0;
            var topics = new[]
            {
                new KafkaTopic { Host = KafkaHosts.Prod, Name = TopicNames.ProdCreated },
                new KafkaTopic { Host = KafkaHosts.Prod, Name = TopicNames.ProdUpdated },
                new KafkaTopic { Host = KafkaHosts.Rc, Name = TopicNames.RcCreated },
                new KafkaTopic { Host = KafkaHosts.Rc, Name = TopicNames.RcUpdated },
            };

            while (true)
            {
                var hosts = topics.Select(t => t.Host).Distinct().ToArray();
                if (hostSelectedIndex == 0 || hostSelectedIndex < 1 || hostSelectedIndex > hosts.Length)
                {
                    Console.WriteLine(Environment.NewLine + "Выберите окружение: ");
                    for (var i = 1; i < hosts.Length + 1; ++i)
                    {
                        Console.WriteLine($"{i}. {hosts[i - 1]}");
                    }

                    var hostSelectedIndexAsString = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (!int.TryParse(hostSelectedIndexAsString.ToString(), out hostSelectedIndex)
                        || hostSelectedIndex < 1
                        || hostSelectedIndex > hosts.Length)
                    {
                        Console.WriteLine("Некорректный индекс.");
                        continue;
                    }
                }

                var topicsBasedOnSelectedHost = topics.Where(t => t.Host == hosts[hostSelectedIndex - 1]).ToArray();
                if (topicNameSelectedIndex < 1 || topicNameSelectedIndex > topicsBasedOnSelectedHost.Length)
                {
                    Console.WriteLine(Environment.NewLine + "Выберите топик: ");
                    for (var i = 1; i < topicsBasedOnSelectedHost.Length + 1; ++i)
                    {
                        Console.WriteLine($"{i}. {topicsBasedOnSelectedHost[i - 1].Name}");
                    }

                    var hostSelectedIndexAsString = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (!int.TryParse(hostSelectedIndexAsString.ToString(), out topicNameSelectedIndex)
                        || topicNameSelectedIndex < 1
                        || topicNameSelectedIndex > topicsBasedOnSelectedHost.Length)
                    {
                        Console.WriteLine("Некорректный индекс.");
                        continue;
                    }
                }

                selectedTopic = topicsBasedOnSelectedHost[topicNameSelectedIndex - 1];

                Console.Write(Environment.NewLine + "Искомый текст: ");
                searchValue = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(searchValue))
                {
                    Console.WriteLine("Некорректное значение для поиска.");
                    continue;
                }

                break;
            }

            return (selectedTopic.Host, selectedTopic.Name, searchValue);
        }

        static int GetGrabMessagesCount()
        {
            const string fileName = "count.txt";
            if (!File.Exists(fileName))
            {
                throw new Exception($"Файл \"{fileName}\" не найден.");
            }

            var fileLines = File.ReadAllLines(fileName);
            if (fileLines.Length == 0)
            {
                throw new Exception($"Файл \"{fileName}\" пустой.");
            }

            if (!int.TryParse(fileLines[0], out var count))
            {
                throw new Exception($"Не удалось распарсить числовое значение из первой строки \"{fileName}\".");
            }

            return count;
        }

        static void SetupApplication()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("consoleapp.log")
                .CreateLogger();

            Console.OutputEncoding = Encoding.UTF8;
        }
    }
}