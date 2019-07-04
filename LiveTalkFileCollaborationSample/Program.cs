/*
 * Copyright 2019 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * システム名：LiveTalkFileCollaborationSample
 * 概要      ：LiveTalkファイル連携サンプルアプリ
*/
using System;
using System.Text.RegularExpressions;

namespace LiveTalkFileCollaborationSample
{
    class Program
    {
        static FileCollaboration FileInterface;
        const string IDTag = " ";

        static void Main(string[] args)
        {
            //var model = new Models.ZinraiFAQModel();
            var param = new string[]
            {
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "LiveTalkOutput.csv"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "AppOutput.txt"),
            };
            if (args.Length >= 1)
            {
                param[0] = args[0];
            }
            if (args.Length >= 2)
            {
                param[1] = args[1];
            }
            Console.WriteLine("InputCSVFileName  :" + param[0]);
            Console.WriteLine("OutputTextFileName:" + param[1]);
            FileInterface = new FileCollaboration(param[0], param[1]);

            // ファイル入力(LiveTalk常時ファイル出力からの入力)
            FileInterface.RemoteMessageReceived += (s) =>
                        {
                            var reg = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                            var items = reg.Split(s);
                            var name = "\"" + System.IO.Path.GetFileNameWithoutExtension(param[1]).ToUpper() + "\"";

                            Console.WriteLine(">>>>>>>");
                            if (items[2].IndexOf(IDTag) == 1 && items[1] == name)
                            {
                                // 自メッセージ出力分なので無視
                            }
                            else
                            {
                                Console.WriteLine("DateTime:" + items[0]);
                                Console.WriteLine("Speaker:" + items[1]);
                                Console.WriteLine("Speech contents:" + items[2]);
                                Console.WriteLine("Translate content:" + items[3]);

                                //Zinrai連携
                                //var answer = model.GetAnswer(items[2]);
                                var answer = "";
                                if (!string.IsNullOrEmpty(answer))
                                {
                                    var answers = answer.Split('\n');
                                    foreach (var item in answers)
                                    {
                                        if (!string.IsNullOrEmpty(item))
                                        {
                                            FileInterface.SendMessage(IDTag + item);
                                        }
                                    }
                                }
                            }
                        };
            FileInterface.WatchFileSart();

            // ファイル出力ループ
            while (true)
            {
                var message = Console.ReadLine();

                if (!string.IsNullOrEmpty(message))
                {
                    // メッセージの出力(LiveTalk常時ファイル入力への出力)
                    message = IDTag + message;
                    FileInterface.SendMessage(message);
                }
                else
                {
                    break;
                }
            }

            FileInterface.WatchFileStop();
        }
    }
}
