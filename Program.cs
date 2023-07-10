using System.IO;
using Renci.SshNet;


class vmupdate
{
	static void Main(string[] args)
	{
	String srvip = "192.168.56.21";
	String srvuser = "oracle";
	String srvpswd = "delta1";
	//String srvip = "carldev.zapto.org";
	//String srvuser = "oracle";
	//String srvpswd = "delta1Delta!";
	string NL = Environment.NewLine;

	stdText("\nVMUPDATE - Carl Last 2023 (C) Capita");

		try
		{
			var ssh = new SshClient(srvip, srvuser, srvpswd);
			Console.WriteLine("Connecting securely to VM...");
			ssh.Connect();
			var result = ssh.RunCommand("echo connected to $HOSTNAME" + NL);
			Console.WriteLine(result.Result);
			result = ssh.RunCommand("rm -f ~/install.sh; sshpass -p \"delta1\" scp  -o ConnectTimeout=10 oracle@10.0.1.14:/backups/vmupdate/install.sh ~/");
			if (result.Error.Length > 0)
			{
				errText(result.Error);
			}
			else
			{
				Console.WriteLine("Running upgrade script...");
				result = ssh.RunCommand("chmod 755 ~/install.sh");
				if (result.Error.Length > 0)
				{
					errText(result.Error);
				}
				else
				{
					var cmd = ssh.CreateCommand("~/install.sh 2>&1");
					var cmdresult = cmd.BeginExecute();
					using (var reader = new StreamReader(cmd.OutputStream))
					{
						using (StreamWriter outputFile = new StreamWriter("vmupdate_log.txt"))
						{
							while (!cmdresult.IsCompleted || !reader.EndOfStream)
							{
								string line = reader.ReadLine();
								if (line != null)
								{
									Console.WriteLine(line);
									outputFile.WriteLine(line);
								}
							}	
						}
					}
					stdText("-= FINISHED =-");
				}
			}
		}
		catch (System.Net.Sockets.SocketException)
		{
			errText("Failed to connect\n");
		}
		catch (Renci.SshNet.Common.SshAuthenticationException)
		{
			errText("Authentication failure\n");
		}
		catch (Renci.SshNet.Common.SshOperationTimeoutException)
		{
			errText("Connection timeout\n");
		}
		catch (Exception e)
		{
			errText(e.ToString());
		}
	}
	
	public static void stdText(string message)
	{
	    ConsoleColor currentForeground = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.DarkBlue;
		Console.WriteLine(message + "\n");
		Console.ForegroundColor = currentForeground;
	}
	
	public static void errText(string message)
	{
		ConsoleColor currentForeground = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine("\n" + message);
		Console.WriteLine("***** PROCESS ABORTED *****\n");
		Console.ForegroundColor = currentForeground;		
	}
}
