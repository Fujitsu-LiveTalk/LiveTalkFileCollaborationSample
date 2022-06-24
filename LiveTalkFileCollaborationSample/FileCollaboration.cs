/*
 * Copyright 2022 FUJITSU LIMITED
 * クラス名　：FileCollaboration
 * 概要      ：LiveTalkの常時ファイル入力/常時ファイル出力と連携
*/
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LiveTalk
{
    internal class FileCollaboration
    {
        private FileSystemWatcher Watcher;
        private object LockObject = new object();
        private string InputFileName = string.Empty;
        private string OutputFileName = string.Empty;

        public FileCollaboration(string inputFileName, string outputFileName)
        {
            this.InputFileName = inputFileName;
            this.OutputFileName = outputFileName;

        }

        /// <summary>
        /// ファイル監視を開始する
        /// </summary>
        public void WatchFileStart()
        {
            if (string.IsNullOrEmpty(this.InputFileName))
            {
                return;
            }
            lock (this.LockObject)
            {
                this.Watcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(this.InputFileName),
                    Filter = Path.GetFileName(this.InputFileName),
                    IncludeSubdirectories = false,
                    NotifyFilter = NotifyFilters.LastWrite,
                };
                this.Watcher.Changed += (s, o) =>
                {
                    FileInput(o.FullPath);
                };
                this.Watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// ファイル監視を終了する
        /// </summary>
        public void WatchFileStop()
        {
            lock (this.LockObject)
            {
                {
                    this.Watcher.EnableRaisingEvents = false;
                    this.Watcher.Dispose();
                    this.Watcher = null;
                }
            }
        }

        private async void FileInput(string fullPathName)
        {
            // 循環入力にならないように同一ファイル名ではないことをチェックする
            if (this.OutputFileName.ToLower() == fullPathName.ToLower())
            {
                return;
            }

            // 更新が終わるのを待つ
            while (System.IO.File.Exists(fullPathName))
            {
                try
                {
                    var destFileName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    System.IO.File.Move(fullPathName, destFileName);
                    try
                    {
                        // ファイルからの入力は非同期に実施する
                        await Task.Run(() =>
                        {
                            var intputFileName = destFileName;
                            try
                            {
                                using (var saveFile = new System.IO.FileStream(intputFileName, FileMode.Open, FileAccess.Read, FileShare.None))
                                {
                                    using (Stream fs = saveFile)
                                    {
                                        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                                        {
                                            // ファイルの終わりまで入力する
                                            while (!sr.EndOfStream)
                                            {
                                                var message = sr.ReadLine();
                                                OnMessageReceived(message);
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                            finally
                            {
                                // 入力し終わった一時ファイルは削除する
                                System.IO.File.Delete(intputFileName);
                            }
                        });
                    }
                    catch { }
                    break;
                }
                catch
                {
                    // ファイルつかみっぱなしでMOVEできないなら１秒待ってリトライ
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// ファイルに１行出力する
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            //オブジェクトがメッセージパネルの場合
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(this.OutputFileName))
            {
                using (var saveFile = new System.IO.FileStream(this.OutputFileName, FileMode.Append, FileAccess.Write, FileShare.Delete | FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(saveFile, Encoding.UTF8))
                    {
                        sw.WriteLine(message);
                        sw.Flush();
                        sw.Close();
                    }
                    saveFile.Close();
                }
            }
        }

        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler RemoteMessageReceived;

        protected virtual void OnMessageReceived(string data)
        {
            this.RemoteMessageReceived?.Invoke(data);
        }
    }
}
