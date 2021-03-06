﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

using MSharp;
using MSharp.Core.Utility;
using MSharp.Entity;
using System.Drawing;

namespace MSharpSample
{
	public partial class SampleForm : Form
	{
		private Misskey mi { set; get; }
		private StatusStorage statusStorage { set; get; }

		public SampleForm()
		{
			InitializeComponent();
		}

		private async void SampleForm_Load(object sender, EventArgs e)
		{
			var authForm = new AuthForm();

			if (authForm.ShowDialog() == System.Windows.Forms.DialogResult.OK && authForm.Result.IsAuthorized)
			{
				mi = authForm.Result;
				statusStorage = new StatusStorage(mi);
			}
			else
				this.Close();

			foreach (var status in await statusStorage.GetNewTimelineStatuses(50))
				listView1.Items.Insert(0, StatusStorage.BuildListViewItem(status));

			var timer = new Timer();
			timer.Interval = 1000 * 5;
			timer.Tick += async (_,__) =>
			{
				foreach (var status in await statusStorage.GetNewTimelineStatuses())
					listView1.Items.Insert(0, StatusStorage.BuildListViewItem(status));
			};
			timer.Start();
        }

		private async void StatusUpdateButton_Click(object sender, EventArgs e)
		{
			try
			{
				var res = await mi.Request(
					MethodType.POST,
					"status/update",
					new Dictionary<string, string> {
						{ "text", StatusUpdateBox.Text }
					});

				StatusUpdateBox.Text = "";
			}
			catch (MSharp.Core.RequestException ex)
			{
				if (ex.Message == "Misskeyからエラーが返されました。")
				{
					var json = Json.Parse((string)ex.Data["Error"]);
					string message = json.message;
					MessageBox.Show(message);
				}
				else
				{
					MessageBox.Show(ex.Message);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}
}
