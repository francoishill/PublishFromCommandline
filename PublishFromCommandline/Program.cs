using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SharedClasses;
using System.IO;
using System.Threading;

class Program
{
	private static T GetArgVal<T>(T defaultIfNotFound, string[] arguments, Func<string, bool> checkIfIsArgument, Func<string, T> functionToGetValueFromArg)
	{
		foreach (string arg in arguments)
			if (!string.IsNullOrWhiteSpace(arg) && checkIfIsArgument(arg))
				return functionToGetValueFromArg(arg);
		return defaultIfNotFound;
	}

	private const StringComparison culture = StringComparison.InvariantCultureIgnoreCase;
	static int Main(string[] args)
	{
		List<string> feedbackList = new List<string>();
		string outputfile = null;

		try
		{
			if (args.Length == 0)
			{
				System.Windows.Forms.Application.EnableVisualStyles();
				//UserMessages.ShowInfoMessage("Please enter arguments for PublishFromCommandLine.");
				feedbackList.Add("No arguments for PublishFromCommandLine.");
				return 0;
			}

			//using (var prog = new SharedClasses.MiniProgressIndeterminateForm("Busy publishing", true))
			//{
			typeof(System.Windows.Forms.Form).GetField("defaultIcon", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
				.SetValue(null, new System.Drawing.Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("PublishFromCommandline.app.ico")));

			string appname = GetArgVal<string>(null, args, s => s.StartsWith("app:", culture), s => s.Substring("app:".Length));
			bool local = GetArgVal<bool>(false, args, s => s.StartsWith("local:", culture), s => !s.Substring("local:".Length).Equals("false", culture));
			bool updaterevision = GetArgVal<bool>(true, args, s => s.StartsWith("updaterevision:", culture), s => !s.Substring("updaterevision:".Length).Equals("false", culture));
			bool hasplugins = GetArgVal<bool>(false, args, s => s.StartsWith("hasplugins:"), s => !s.Substring("hasplugins:".Length).Equals("false", culture));
			bool loadonstartup = GetArgVal<bool>(true, args, s => s.StartsWith("loadonstartup:"), s => !s.Substring("loadonstartup:".Length).Equals("false", culture));
			bool installlocal = GetArgVal<bool>(false,/*true,*/ args, s => s.StartsWith("installlocal:"), s => !s.Substring("installlocal:".Length).Equals("false", culture));
			bool selectsetup = GetArgVal<bool>(false, args, s => s.StartsWith("selectsetup:"), s => !s.Substring("selectsetup:".Length).Equals("false", culture));
			bool openwebsite = GetArgVal<bool>(false, args, s => s.StartsWith("openwebsite:"), s => !s.Substring("openwebsite:".Length).Equals("false", culture));
			outputfile = GetArgVal<string>(null, args, s => s.StartsWith("outputfile:"), s => s.Substring("outputfile:".Length));

			if (appname == null)
				feedbackList.Add("Cannot publish, no appname specified (use command-line format app:\"my app name\")");
			string versionString;
			if (local)
				VisualStudioInterop.PerformPublish(
					null,
					appname,
					false,
					out versionString,
					hasplugins,
					true,
					installlocal,
					loadonstartup,
					(sn, tf) => { feedbackList.Add(DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]{" + tf.FeedbackType + "} " + tf.FeedbackText)); },
					selectsetup);
			else
			{
				//System.Windows.Forms.Application.EnableVisualStyles();
				//MiniDownloadBarForm.UpdateMessage("Busy publishing app " + appname, null);

				VisualStudioInterop.PerformPublishOnline(
					null,
					appname,
					false,
					hasplugins,
					true,
					loadonstartup,
					(sn, textfeedback) =>
					{
						feedbackList.Add("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "[{" + textfeedback.FeedbackType + "} " + textfeedback.FeedbackText);
						//MiniDownloadBarForm.UpdateMessage(textfeedback.FeedbackText, "Last feedback:" + textfeedback.FeedbackText);
					},
					(sn, progressfeedback) =>
					{
						//MiniDownloadBarForm.UpdateProgress((int)Math.Truncate(100 * ((double)progressfeedback.CurrentValue / (double)progressfeedback.MaximumValue)));
					},
					installlocal,
					selectsetup,
					openwebsite);

				//MiniDownloadBarForm.CloseDownloadBar();
			}

			//string appname = args[0];
			//bool local = args.Length < 2 ? true : (args[1].Equals("local", culture));
			//bool hasplugins
			//}
		}
		finally
		{
			if (feedbackList.Count > 0)
			{
				if (outputfile == null)//No user specified path
					outputfile = Path.GetTempPath().TrimEnd('\\') + "\\Published feedback (" + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + ").txt";
				File.WriteAllLines(outputfile, feedbackList);
				Process.Start("explorer", "/select,\"" + outputfile + "\"");
			}
		}

		return 0;
	}
}