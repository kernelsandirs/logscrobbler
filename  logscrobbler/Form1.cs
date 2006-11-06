/*
 * Created by SharpDevelop.
 * User: timg
 * Date: 9/29/2006
 * Time: 10:16 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Audioscrobbler.NET;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Net;
using System.ComponentModel;


namespace LogScrobbler
{
	/// <summary>
	/// Description of Form1.
	/// </summary>
	public partial class Form1
	{
		public string swColor;
		public string myDir = Directory.GetCurrentDirectory();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
		public Form1()
		{
			
			InitializeComponent();
			try{

				StreamReader sett = new StreamReader(myDir + "\\LogScrobbler.txt");
				string setting;
				string[] fields;
				int count = 0;
				while ((setting = sett.ReadLine()) != null)
				{
					fields = setting.Split('=');
					if(count == 0) {
						textBox2.Text = fields[1];
					}
					if(count == 1) {
						textBox3.Text = fields[1];
					}
					if(count == 2) {
						textBox1.Text = fields[1];
					}
					if(count == 3) {
						checkBox1.Checked = Convert.ToBoolean(fields[1]);
					}
					if(count == 4) {
						checkBox2.Checked = Convert.ToBoolean(fields[1]);
					}
					if(count == 5) {
						swColor = fields[1];
					}
					if(count == 6) {
						checkBox3.Checked = Convert.ToBoolean(fields[1]);
					}
					if(count == 7) {
						checkBox4.Checked = Convert.ToBoolean(fields[1]);
					}
					if(count == 8) {
						checkBox5.Checked = Convert.ToBoolean(fields[1]);
					}
					if(count == 9) {
						checkBox6.Checked = Convert.ToBoolean(fields[1]);
					}
					count++;

					
				}
				sett.Close();
				checkReq();
				
			}
			catch
			{
				
			}
			if(swColor == "sw2") {
				button8.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.sw2");
				this.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.bg2");
				swColor = "sw2";
			} else {
				button8.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.sw1");
				this.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.BackgroundImage");
				swColor = "sw1";
			}
			if(checkBox4.Checked == true){
				getMyImage();
			}
		}

		void getMyImage()
		{
			try
			{
				System.Net.WebClient Client = new WebClient();
				Stream strm = Client.OpenRead("http://www.last.fm/user/"+ textBox2.Text +"/");
				StreamReader sr = new StreamReader(strm);
				string line;
				string oktoGo = "";
				string oktoStop = "";
				string title = "";
				do
				{
					line = sr.ReadLine();
					Regex regex1 = new Regex("avatarPanel");
					if(regex1.IsMatch(line)) {
						oktoGo = "OK";
					}
					
					Regex regex2= new Regex("static.last.fm/avatar");
					if(regex2.IsMatch(line)) {
						if (oktoGo == "OK" && oktoStop != "OK"){
							string match = ".*<img src=\"";
							line = Regex.Replace(line,match,"");
							string match2 = "\" alt=\".*";
							line = Regex.Replace(line,match2,"");
							title = line.Replace("<img src=\"http://static.last.fm/avatar/","");
							title = title.Trim();
							pictureBox1.WaitOnLoad = false;
							pictureBox1.LoadAsync(@title);
						}
					}
					
					Regex regexstp = new Regex("</a>");
					if(regexstp.IsMatch(line)) {
						if (oktoGo == "OK"){
							oktoStop = "OK";
						}
					}
				}
				while (line !=null);
				strm.Close();
				

			}
			catch
			{
				
			}
			
		}
		
		void Button2Click(object sender, System.EventArgs e)
		{
			openFileDialog1.ShowDialog();
		}
		
		void OpenFileDialog1FileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			textBox1.Text = openFileDialog1.FileName;
			checkReq();
			checkForFile();
			listView1.Items.Clear();
			getList();
		}
		
		void checkReq(){
			if(textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "") {
				button1.Enabled = true;
				button3.Enabled = true;
			} else {
				button1.Enabled = false;
				button3.Enabled = false;
			}
		}

		
		void Button1Click(object sender, System.EventArgs e)
		{

			Track track = new Track();
			AudioscrobblerRequest AS = new AudioscrobblerRequest();
			AudioscrobblerException AE = new AudioscrobblerException();
			AS.Username = textBox2.Text.ToString();
			AS.Password = textBox3.Text.ToString();
			int countChecked =0;
			try
			{
				if(checkBox6.Checked == true) {
					zeroStamps();
				}
				foreach (System.Windows.Forms.ListViewItem itemRow in listView1.CheckedItems)
				{
					countChecked++;
				}

				double stepSize = 0;
				stepSize = 100 / countChecked;
				progressBar1.Step = Convert.ToInt32(stepSize);
				
				foreach (System.Windows.Forms.ListViewItem itemRow in listView1.CheckedItems)
				{
					for( int counter = 0; counter < itemRow.SubItems.Count; counter++ )
					{
						track.ArtistName = itemRow.SubItems[0].Text;
						track.TrackName = itemRow.SubItems[1].Text;
						track.AlbumName = itemRow.SubItems[2].Text;
						//	track.TrackLength = Convert.ToInt32(itemRow.SubItems[3].Text);
						string [] mins = new string[2];
						char[] splitter  = {':'};
						mins = itemRow.SubItems[3].Text.Split(splitter);
						track.TrackLength = (Convert.ToInt32(mins[0]) * 60 + Convert.ToInt32(mins[1]));
						track.TimePlayed = itemRow.SubItems[4].Text;
						
					}
					//MessageBox.Show(track.ArtistName + " " +track.TrackName + " " +track.AlbumName + " " +track.TrackLength + " " + track.TimePlayed);
					AS.SubmitTrack(track);
					progressBar1.Value = progressBar1.Value + Convert.ToInt32(stepSize);
				}

				MessageBox.Show("Sync Complete.");
				if(checkBox1.Checked) {
					File.Delete(textBox1.Text.ToString());
					listView1.Items.Clear();
				}
				if(checkBox2.Checked) {
					Application.Exit();
				}
			}
			catch
			{
				MessageBox.Show("Nothing to sync, or there was an error connecting to Last.fm");
			}
			saveSettings();
			checkForFile();
			progressBar1.Value = 0;
		}
		

		void saveSettings(){
			StreamWriter sw = new StreamWriter(myDir + "\\LogScrobbler.txt");
			sw.Write("Username=" + textBox2.Text.ToString());
			sw.Write("\r\n");
			if(checkBox5.Checked == true) {
				sw.Write("Password=" + textBox3.Text.ToString());
			} else {
				sw.Write("Password=");
			}
			sw.Write("\r\n");
			sw.Write("Path=" + textBox1.Text.ToString());
			sw.Write("\r\n");
			sw.Write("Delete=" + checkBox1.Checked.ToString());
			sw.Write("\r\n");
			sw.Write("Exit=" + checkBox2.Checked.ToString());
			sw.Write("\r\n");
			sw.Write("Color=" + swColor.ToString());
			sw.Write("\r\n");
			sw.Write("ExitApp=" + checkBox3.Checked.ToString());
			sw.Write("\r\n");
			sw.Write("ShowAvatar=" + checkBox4.Checked.ToString());
			sw.Write("\r\n");
			sw.Write("SavePass=" + checkBox5.Checked.ToString());
			sw.Write("\r\n");
			sw.Write("IgnoreStamps=" + checkBox6.Checked.ToString());
			sw.Close();
		}
		
		void LinkLabel1LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			ProcessStartInfo sInfo = new ProcessStartInfo(linkLabel1.Text.ToString());
			Process.Start(sInfo);
		}
		
		void TextBox2TextChanged(object sender, System.EventArgs e)
		{
			linkLabel1.Text = "http://www.last.fm/user/"+textBox2.Text.ToString();
		}
		
		void TextBox3KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			checkReq();
		}
		
		void TextBox2KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			checkReq();
		}
		
		void Form1Load(object sender, System.EventArgs e)
		{
			linkLabel1.Links.Remove(linkLabel1.Links[0]);
			linkLabel1.Links.Add(0, linkLabel1.Text.Length, "http://www.last.fm/user/"+textBox2.Text.ToString());
			checkForFile();
		}
		
		
		void checkForFile(){
			if(File.Exists(textBox1.Text.ToString()))
			{
				button1.Enabled = true;
				button3.Enabled = true;
				label4.Text = "";
				listView1.Items.Clear();
				getList();
			} else {
				button1.Enabled = false;
				button3.Enabled = false;
				label4.Text = "scrobbler log not found";
			}
			
		}
		
		void getList()
		{
			string FILELINE;
			string[] fields;
			string trackStatus = "";
			int Tmin=0;
			int Tsec=0;
			StreamReader log = new StreamReader(textBox1.Text.ToString());
			while ((FILELINE = log.ReadLine()) != null)
			{
				fields = FILELINE.Split('\t');
				Regex regex = new Regex("#");
				if(!regex.IsMatch(fields[0])) {
					trackStatus = fields[5];
					if(trackStatus == "L") {
						ListViewItem item = listView1.Items.Add(fields[0]);
						item.Checked = true;
						item.SubItems.Add(fields[2]);
						item.SubItems.Add(fields[1]);
						Tmin = Convert.ToInt32(fields[4]) / 60;
						Tsec = Convert.ToInt32(fields[4]) % 60;
						item.SubItems.Add(Tmin + ":" + String.Format("{0:00}",Tsec));
						//item.SubItems.Add(fields[4]);
						item.SubItems.Add((new DateTime(1970,1,1,0,0,0)).AddSeconds(Convert.ToDouble(fields[6])).ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
					}
				}
			}
			log.Close();
			int rowcolor=0;
			int rc=0;
			foreach (System.Windows.Forms.ListViewItem itemRow in listView1.Items)
			{
				rowcolor++;
				rc=rowcolor%2;
				if(rc == 0){
					itemRow.BackColor = System.Drawing.Color.FromArgb(234,234,240);
				} else {
					itemRow.BackColor = System.Drawing.Color.White;
				}
				
			}
		}
		
		void Button3Click(object sender, System.EventArgs e)
		{
			this.Size = new System.Drawing.Size(650, 600);
			listView1.Items.Clear();
			listView1.Visible = true;
			getList();
		}
		
		void Button4Click(object sender, System.EventArgs e)
		{
			this.Size = new System.Drawing.Size(615, 300);
			listView1.Visible = false;
		}

		
		void Button6Click(object sender, System.EventArgs e)
		{
			MyLastTen lastTen = new MyLastTen(myDir);
			lastTen.ShowDialog();
		}
		
		void Button5Click(object sender, System.EventArgs e)
		{
			// Shift to after last played track.
			try
			{
				//http://ws.audioscrobbler.com/1.0/user/kernelsandirs/recenttracks.rss
				System.Net.WebClient Client = new WebClient();
				Stream strm = Client.OpenRead("http://ws.audioscrobbler.com/1.0/user/" + textBox2.Text.ToString() + "/recenttracks.rss");
				StreamReader sr = new StreamReader(strm);
				string line;
				string lastEntry = "";
				string firstInlog = "";
				string oktoGo = "";
				
				do
				{
					line = sr.ReadLine();
					Regex regex1 = new Regex("description");
					if(regex1.IsMatch(line)) {
						oktoGo = "OK";
					}
					Regex regex = new Regex("pubDate");
					if(lastEntry == "") {
						if(regex.IsMatch(line)) {
							if(oktoGo == "OK"){
								lastEntry = line.Replace("<pubDate>","");
								lastEntry = lastEntry.Replace("</pubDate>","");
								lastEntry = lastEntry.Replace(lastEntry.Substring(lastEntry.Length-5,5),"");
								lastEntry = lastEntry.Trim();
								DateTime start = (DateTime)(TypeDescriptor.GetConverter(new DateTime(1990,5,6)).ConvertFrom(lastEntry));
								firstInlog = listView1.CheckedItems[0].SubItems[4].Text;
								DateTime end = (DateTime)(TypeDescriptor.GetConverter(new DateTime(1990,5,6)).ConvertFrom(firstInlog));
								long MinutesDiff = start.Ticks - end.Ticks;
								MinutesDiff = (MinutesDiff / 10000000) / 60 + 3;
								if(MinutesDiff > 0){
									foreach (System.Windows.Forms.ListViewItem itemRow in listView1.CheckedItems)
									{
										DateTime listtime = (DateTime)(TypeDescriptor.GetConverter(new DateTime(1990,5,6)).ConvertFrom(itemRow.SubItems[4].Text));
										listtime = listtime.AddMinutes(MinutesDiff);
										itemRow.SubItems[4].Text = listtime.Year + "-" + listtime.Month + "-" + listtime.Day + " " + listtime.TimeOfDay;
									}
								} else {
									MessageBox.Show("Time of last played track is less than the first track in your list, Time-shift not needed");
								}
							}
						}
					}
				}
				while (line !=null);
				strm.Close();
			}
			catch
			{
				
			}
		}
		
		void Button7Click(object sender, System.EventArgs e)
		{
			saveSettings();
		}
		

		
		void Button8Click(object sender, System.EventArgs e)
		{
			if(swColor == "sw2") {
				this.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.BackgroundImage");
				button8.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.sw1");
				swColor = "sw1";
			} else {
				this.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.bg2");
				button8.BackgroundImage =
					(System.Drawing.Bitmap)resources.GetObject("$this.sw2");
				swColor = "sw2";
			}
			saveSettings();
		}
		
		void Form1FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
		{
			if(checkBox3.Checked == true) {
				saveSettings();
				Application.Exit();
			}
		}
		
		void PictureBox1Click(object sender, System.EventArgs e)
		{
			ProcessStartInfo sInfo = new ProcessStartInfo(linkLabel1.Text.ToString());
			Process.Start(sInfo);
		}
		
		
		void ListView1ItemChecked(object sender, System.Windows.Forms.ItemCheckedEventArgs e)
		{
			foreach (System.Windows.Forms.ListViewItem itemRow in listView1.Items)
			{
				if(itemRow.Checked == false) {
					itemRow.ForeColor = System.Drawing.Color.Gray;
				} else {
					itemRow.ForeColor = System.Drawing.Color.DarkGreen;
				}
			}
		}
		
		void Button9Click(object sender, System.EventArgs e)
		{
			zeroStamps();
		}
		
		public void zeroStamps()
		{
			try
			{
				int total=0;
				foreach (System.Windows.Forms.ListViewItem itemRow in listView1.CheckedItems)
				{
					string [] mins = new string[2];
					char[] splitter  = {':'};
					mins = itemRow.SubItems[3].Text.Split(splitter);
					total = (Convert.ToInt32(mins[0]) * 60 + Convert.ToInt32(mins[1])) + total;
				}
				DateTime listtime = (DateTime)(TypeDescriptor.GetConverter(new DateTime(1990,5,6)).ConvertFrom(DateTime.Now.ToUniversalTime().ToString()));
				foreach (System.Windows.Forms.ListViewItem itemRow in listView1.CheckedItems)
				{
					string [] mins = new string[2];
					char[] splitter  = {':'};
					mins = itemRow.SubItems[3].Text.Split(splitter);
					int fixtime = (Convert.ToInt32(mins[0]) * 60 + Convert.ToInt32(mins[1]));
					listtime = listtime.AddSeconds(-1*fixtime);
					itemRow.SubItems[4].Text = listtime.Year + "-" + listtime.Month + "-" + listtime.Day + " " + listtime.TimeOfDay;
					
					
				}
				
			}
			catch
			{
				
			}
			
		}
	}

	public class Track : IAudioscrobblerTrack
	{
		string a_text = "";
		string t_text = "";
		string al_text = "";
		string m_text = "";
		string tp_text = "";
		int tl_int;
		public Track()
		{}
		public string ArtistName
		{
			get
			{
				return a_text;
			}

			set
			{
				a_text = value;
			}
		}
		public string TrackName
		{
			get
			{
				return t_text;
			}

			set
			{
				t_text = value;
			}
		}
		public string AlbumName
		{
			get
			{
				return al_text;
			}

			set
			{
				al_text = value;
			}
		}
		public string MusicBrainzID
		{
			get
			{
				return m_text;
			}

			set
			{
				m_text = value;
			}
		}
		public string TimePlayed
		{
			get
			{
				return tp_text;
			}

			set
			{
				tp_text = value;
			}
		}
		public int TrackLength
		{
			get
			{
				return tl_int;
			}

			set
			{
				tl_int = value;
			}
		}
	}
}
