using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace GC2Toolkit
{
    public class MainServer
    {
        public int ItemGetCount = 1;
        public HttpListener proc;
        string BuildVer = "";

        string ModelPakAdd = "";
        string TuneFilePakAdd = "";
        string SkinPakAdd = "";

        public int MaxStage = 400;
        public int MaxAvater = 90;

		private string CacheFolder = "Caches\\";
		private bool nonWindows = false;

        public MainServer(string BuildVersion)
        {
            BuildVer = BuildVersion;
            if (!Directory.Exists("Caches"))
                Directory.CreateDirectory("Caches");

			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
			{
				Console.WriteLine("Setting up for non-Windows environment...");
				CacheFolder = "Caches/";
				nonWindows = true;
			}
            proc = new HttpListener();
            try
            {
                proc.Prefixes.Add("http://*:80/");
                proc.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine("Binded to port 80 failed.");
                throw e;
            }
        }

        public void ResetTimer()
        {
            ItemGetCount = 1;
            Console.WriteLine("Login timer reseted to Day 1.");
            Console.Title = "GC2 FullUnlock Toolkit | Login Day:" + ItemGetCount.ToString();
        }

        public void SetTimer(int Day)
        {
            ItemGetCount = Day;
            Console.WriteLine("Login timer set to Day " + Day.ToString());
            Console.Title = "GC2 FullUnlock Toolkit | Login Day:" + ItemGetCount.ToString();
        }

        public void listen()
        {
            while (true)
            {
                IAsyncResult result = proc.BeginGetContext(new AsyncCallback(ListenerCallback), proc);
                result.AsyncWaitHandle.WaitOne();
            }

        }

        private void ListenerCallback(IAsyncResult result)
        {
            HttpListenerContext context = proc.EndGetContext(result);
            string RequestFile = context.Request.Url.AbsolutePath;
            string RequestParam = "";
            if(context.Request.Url.Query.Length > 0)
                RequestParam = context.Request.Url.Query.Substring(1);

			if (nonWindows)
			{
				if (!File.Exists("Caches/" + RequestFile) || (RequestFile.Contains("start.php")) || (RequestFile.Contains("sync.php")) || RequestFile.Contains("info.php") || RequestFile.Contains("briefing.php"))
					try
					{
						GatherFile(RequestFile, RequestParam, context.Request);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						return;
					}
			}
			else {
				if (!File.Exists("Caches\\" + RequestFile) || (RequestFile.Contains("start.php")) || (RequestFile.Contains("sync.php")) || RequestFile.Contains("info.php") || RequestFile.Contains("briefing.php"))
					try
					{
						GatherFile(RequestFile, RequestParam, context.Request);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						return;
					}
			}

			byte[] Response = File.ReadAllBytes(CacheFolder + RequestFile);

            Console.WriteLine("Sending:" + RequestFile);
            try
            {
                context.Response.OutputStream.Flush();
                context.Response.OutputStream.Write(Response, 0, Response.Length);
            }
            catch
            {
                Console.WriteLine("Connection to client dropped when sending.");
                context.Response.Close();
                return;
            }
            context.Response.Close();

            //Console.WriteLine(RequestFile + " " + RequestParam);
            return;
        }
        
        public void PakUpdater()
        {
            if(ModelPakAdd == "" || TuneFilePakAdd == "" || SkinPakAdd == "")
            {
                Console.WriteLine("Please open game first to get pak address.");
                return;
            }
            foreach(string PakAdd in new string[] {ModelPakAdd,TuneFilePakAdd,SkinPakAdd})
            {
                string[] PakFilename = PakAdd.Split('/');
				if (!File.Exists(CacheFolder + PakFilename[PakFilename.Length - 1]))
                {
                    PakDownloader(ModelPakAdd, CacheFolder + PakFilename[PakFilename.Length - 1], PakFilename[PakFilename.Length - 1]);
                }
                else Console.WriteLine("Pak already up-to-date:" + PakFilename[PakFilename.Length - 1]);
            }
        }

        bool DownloadCompleted = false;
        private void PakDownloader(string ReqAddress,string SaveDest,string Filename)
        {
            DownloadCompleted = false;
            Console.WriteLine("Starting download of:" + Filename);
            WebClient DownloadClient = new WebClient();
            DownloadClient.DownloadProgressChanged += DownloadClient_DownloadProgressChanged;
            DownloadClient.DownloadFileCompleted += DownloadClient_DownloadFileCompleted;
            DownloadClient.DownloadFileAsync(new Uri(ReqAddress), SaveDest);
            while (!DownloadCompleted)
            {

            }
        }

        private void DownloadClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Console.WriteLine("Download completed.");
            DownloadCompleted = true;
        }

        private void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.Title = "GC2 FullUnlock Toolkit | Progress:" + e.BytesReceived + "/" + e.TotalBytesToReceive + "(" +e.ProgressPercentage + "%)";
        }

        private void GatherFile(string ReqFile,string ReqParam,HttpListenerRequest SourceReq)
        {
            HttpWebRequest ServerRequest = WebRequest.Create(new Uri("http://gc2.gczero.com/" + ReqFile + "?" + ReqParam)) as HttpWebRequest;

            for(int i = 0;i < SourceReq.Headers.Count; i++)
            {
                string HeaderName_Orig = SourceReq.Headers.Keys[i];
                string HeaderValue_Orig = SourceReq.Headers.GetValues(i)[0];

                switch (HeaderName_Orig)
                {
                    case "Accept":
                        ServerRequest.Accept = HeaderValue_Orig;
                        //ServerRequest.Connection = HeaderValue_Orig;
                        break;
                    case "User-Agent":
                        ServerRequest.UserAgent = HeaderValue_Orig;
                        break;
                    case "Accept-Encoding":
                        //ServerRequest.TransferEncoding = HeaderValue_Orig;
                        break;
                    case "Accept-Language":
                        //ServerRequest.a
                        break;
                    case "Host":
                        ServerRequest.Host = HeaderValue_Orig;
                        break;
                }
            }

            var Input = default(byte[]);
            bool HasContent = false;
            if (SourceReq.HttpMethod == "POST")
            {
                HasContent = true;
                using (var memstream = new MemoryStream())
                {
                    SourceReq.InputStream.CopyTo(memstream);
                    Input = memstream.ToArray();
                }
            }

            if (HasContent)
            {
                ServerRequest.Method = "POST";
                ServerRequest.GetRequestStream().Write(Input, 0, Input.Length);
            }

            Console.WriteLine("Getting response from game server:" + ReqFile);
            HttpWebResponse ServerResponse;
            try
            {
                ServerResponse = ServerRequest.GetResponse() as HttpWebResponse;
            }
            catch(Exception e)
            {
                throw e;
            }

            var ServerOutput = default(byte[]);
            using (var memstream = new MemoryStream())
            {
                ServerResponse.GetResponseStream().CopyTo(memstream);
                ServerOutput = memstream.ToArray();
            }

            if(ReqFile.Split('/').Length > 0)
            {
                string DirectoryData = "";
                foreach(string DirName in ReqFile.Split('/'))
                {
					if (!DirName.Contains("."))
					{
						if(nonWindows)
							DirectoryData += DirName + "/";
						else
							DirectoryData += DirName + "\\";
					}

                    if (!Directory.Exists(DirectoryData))
                    {
                        Console.WriteLine("Creating directory:" + DirectoryData);
						Directory.CreateDirectory(CacheFolder+DirectoryData);
                    }
                }
                

            }
			File.WriteAllBytes(CacheFolder + ReqFile, ServerOutput);
            SpecialTreatment(ReqFile);
            return;
            //Console.ReadLine();
        }

        private void SpecialTreatment(string FileName)
        {
            if (FileName.Contains("web_shop_detail"))
            {
				string FileData = File.ReadAllText(CacheFolder + FileName);
                FileData = FileData.Replace("purchase_money(", "purchase_coin('ja',");

                FileData = FileData.Replace(");\" class=\"btn\"><div class=\"btn_green a_center w40\">Purchase<br>", ",'cnt_type=1&cnt_id=314&num=1');\" class=\"btn\"><div class=\"btn_green a_center w40\">Purchase<br>");

				File.WriteAllText(CacheFolder + FileName,FileData);
                Console.WriteLine("Special Process to " + FileName + " has done.");
                //FileData.Replace()
            }
            if (FileName.Contains("buy_by_coin"))
            {
                string DataToGo = "";
                for (int q = 1; q <= 226; q++)
                    DataToGo += "<stage_id>" + q.ToString() + "</stage_id>\n";

                DataToGo = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><response><code>0</code><result_url>web_shop_result.php</result_url><cnt_type>1</cnt_type><cnt_id>314</cnt_id><num>1</num>" + DataToGo + "</response>";
				File.WriteAllText(CacheFolder + FileName, DataToGo);
                Console.WriteLine("Special Process to " + FileName + " has done.");
            }
            if(FileName.Contains("start") || FileName.Contains("sync"))
            {
				string FileData = File.ReadAllText(CacheFolder + FileName);
                UpdatePakAdd(FileData);
                if (FileName.Contains("start"))
                {
                    if (FileData.Contains("<info>0</info>"))
                        FileData = FileData.Replace("<info>0</info>", "<info>1</info>");
                    else FileData = FileData.Insert(FileData.IndexOf("</is_tutorial>") + 14, "<info>1</info>");
                }
                
                string[] FileData_Contents = FileData.Split(new string[] { "my_stage" },StringSplitOptions.None);
                string FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
                string FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

                string DataToGo = "";
                for (int i = 1; i <= MaxStage; i++)
                    DataToGo += "<my_stage>\n<stage_id>" + i.ToString() + "</stage_id>\n<ac_mode>1</ac_mode>\n</my_stage>\n";
                
                FileData = FileData_FirstPart + DataToGo + FileData_LastPart;

                FileData_Contents = FileData.Split(new string[] { "my_avatar" }, StringSplitOptions.None);
                FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
                FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

                DataToGo = "";
                for (int i = 1; i <= MaxAvater; i++)
				{
					if(i == 87 ||i == 88 || i == 89) continue;
					else DataToGo += "<my_avatar>" + i.ToString() + "</my_avatar>\n";
				}

                FileData = FileData_FirstPart + DataToGo + FileData_LastPart;
                if (FileData.Contains("login_bonus"))
                {
                    Console.Title = "GC2 FullUnlock Toolkit | Login Day:" + ItemGetCount.ToString();

                    FileData_Contents = FileData.Split(new string[] { "login_bonus" }, StringSplitOptions.None);
                    FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
                    FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

                    DataToGo = "<login_bonus><status>1</status><last_count>0</last_count><now_count>" + ItemGetCount.ToString() + "</now_count>" +
                        "<reward><count>1</count><cnt_type>3</cnt_type><cnt_id>1</cnt_id><num>200</num></reward>" +
                        "<reward><count>2</count><cnt_type>3</cnt_type><cnt_id>2</cnt_id><num>200</num></reward>" +
                        "<reward><count>3</count><cnt_type>3</cnt_type><cnt_id>3</cnt_id><num>200</num></reward>" +
                        "<reward><count>4</count><cnt_type>3</cnt_type><cnt_id>4</cnt_id><num>200</num></reward>" +
                        "<reward><count>5</count><cnt_type>3</cnt_type><cnt_id>5</cnt_id><num>200</num></reward>" +
                        "<reward><count>6</count><cnt_type>3</cnt_type><cnt_id>6</cnt_id><num>200</num></reward>" +
                        "<reward><count>7</count><cnt_type>3</cnt_type><cnt_id>7</cnt_id><num>200</num></reward>" +
                        "<reward><count>8</count><cnt_type>3</cnt_type><cnt_id>8</cnt_id><num>200</num></reward>" +
                        "<reward><count>9</count><cnt_type>3</cnt_type><cnt_id>9</cnt_id><num>200</num></reward>" +
                        "<reward><count>10</count><cnt_type>3</cnt_type><cnt_id>10</cnt_id><num>200</num></reward>" +
                        "<reward><count>26</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                        "<reward><count>27</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                        "<reward><count>28</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                        "<reward><count>29</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                        "<reward><count>30</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                        "<count_update>1</count_update><message>You are now connected to GC2 Unlock tool.\nCurrent Day:" + ItemGetCount.ToString() + "\nPs:Restart game to get next item.</message>" + "</login_bonus>";

                    ItemGetCount++;
                    FileData = FileData_FirstPart + DataToGo + FileData_LastPart;
                }

				File.WriteAllText(CacheFolder + FileName, FileData);
                Console.WriteLine("Special Process to " + FileName + " has done.");
            }
            if (FileName.Contains("info"))
            {
				string FileData = File.ReadAllText(CacheFolder + FileName);
                string DataToInsert = "<a style=\"display:block\" class=\"f80  mtb10\">GC2 FullUnlock Mod Online.<br/>" + BuildVer + "</a>";
                FileData = FileData.Insert(FileData.IndexOf("<ul class=\"listbox f80 mtb10\">") + 31, DataToInsert);

				File.WriteAllText(CacheFolder + FileName, FileData);
                Console.WriteLine("Special Process to " + FileName + " has done.");
            }
        }

        private void UpdatePakAdd(string StartData)
        {
            string[] PartToGo = new string[] { "model_pak", "tuneFile_pak", "skin_pak" };
            string Output = "";
            foreach(string Part in PartToGo)
            {
                string[] DataPart = StartData.Split(new string[] { Part }, StringSplitOptions.None);
                string[] URLPart = DataPart[1].Split(new string[] { "url" }, StringSplitOptions.None);
                string URLData = URLPart[1].Substring(1, URLPart[1].Length - 3);

                switch (Part)
                {
                    case "model_pak":
                        ModelPakAdd = URLPart[1].Substring(1, URLPart[1].Length - 3);
                        break;
                    case "tuneFile_pak":
                        TuneFilePakAdd = URLPart[1].Substring(1, URLPart[1].Length - 3);
                        break;
                    case "skin_pak":
                        SkinPakAdd = URLPart[1].Substring(1, URLPart[1].Length - 3);
                        break;
                }
                Output += Part + "->" + URLPart[1].Substring(1, URLPart[1].Length - 3) + "\n";
            }
            Console.WriteLine(Output);
			if(nonWindows)
				File.WriteAllText("./PakFileAdd.txt", Output);
			else
				File.WriteAllText(".\\PakFileAdd.txt", Output);
        }

        public void GenerateDummyResponse(string FilePath){
            string FileData = File.ReadAllText(FilePath);

            if (FileData.Contains("<info>0</info>"))
                FileData = FileData.Replace("<info>0</info>", "<info>1</info>");
            else FileData = FileData.Insert(FileData.IndexOf("</is_tutorial>") + 14, "<info>1</info>");

            string[] FileData_Contents = FileData.Split(new string[] { "my_stage" }, StringSplitOptions.None);
            string FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
            string FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

            string DataToGo = "";
            for (int i = 1; i <= MaxStage; i++)
                DataToGo += "<my_stage>\n<stage_id>" + i.ToString() + "</stage_id>\n<ac_mode>1</ac_mode>\n</my_stage>\n";

            FileData = FileData_FirstPart + DataToGo + FileData_LastPart;

            FileData_Contents = FileData.Split(new string[] { "my_avatar" }, StringSplitOptions.None);
            FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
            FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

            DataToGo = "";
            for (int i = 1; i <= MaxAvater; i++)
            {
                if (i == 87 || i == 88 || i == 89) continue;
                else DataToGo += "<my_avatar>" + i.ToString() + "</my_avatar>\n";
            }

            FileData = FileData_FirstPart + DataToGo + FileData_LastPart;
            if (FileData.Contains("login_bonus"))
            {
                Console.Title = "GC2 FullUnlock Toolkit | Login Day:" + ItemGetCount.ToString();

                FileData_Contents = FileData.Split(new string[] { "login_bonus" }, StringSplitOptions.None);
                FileData_FirstPart = FileData_Contents[0].Substring(0, FileData_Contents[0].Length - 1);
                FileData_LastPart = FileData_Contents[FileData_Contents.Length - 1].Substring(1);

                DataToGo = "<login_bonus><status>1</status><last_count>0</last_count><now_count>" + ItemGetCount.ToString() + "</now_count>" +
                    "<reward><count>1</count><cnt_type>3</cnt_type><cnt_id>1</cnt_id><num>200</num></reward>" +
                    "<reward><count>2</count><cnt_type>3</cnt_type><cnt_id>2</cnt_id><num>200</num></reward>" +
                    "<reward><count>3</count><cnt_type>3</cnt_type><cnt_id>3</cnt_id><num>200</num></reward>" +
                    "<reward><count>4</count><cnt_type>3</cnt_type><cnt_id>4</cnt_id><num>200</num></reward>" +
                    "<reward><count>5</count><cnt_type>3</cnt_type><cnt_id>5</cnt_id><num>200</num></reward>" +
                    "<reward><count>6</count><cnt_type>3</cnt_type><cnt_id>6</cnt_id><num>200</num></reward>" +
                    "<reward><count>7</count><cnt_type>3</cnt_type><cnt_id>7</cnt_id><num>200</num></reward>" +
                    "<reward><count>8</count><cnt_type>3</cnt_type><cnt_id>8</cnt_id><num>200</num></reward>" +
                    "<reward><count>9</count><cnt_type>3</cnt_type><cnt_id>9</cnt_id><num>200</num></reward>" +
                    "<reward><count>10</count><cnt_type>3</cnt_type><cnt_id>10</cnt_id><num>200</num></reward>" +
                    "<reward><count>26</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                    "<reward><count>27</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                    "<reward><count>28</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                    "<reward><count>29</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                    "<reward><count>30</count><cnt_type>1</cnt_type><cnt_id>277</cnt_id><num>1</num></reward>" +
                    "<count_update>1</count_update><message>You are now connected to GC2 Unlock tool.\nCurrent Day:" + ItemGetCount.ToString() + "\nPs:Restart game to get next item.</message>" + "</login_bonus>";

                ItemGetCount++;
                FileData = FileData_FirstPart + DataToGo + FileData_LastPart;
            }

            File.WriteAllText(FilePath + ".output", FileData);
            Console.WriteLine("Special Process to " + FilePath + " has done.");
        }
    }
}
