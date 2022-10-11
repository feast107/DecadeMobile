using Org.Apache.Http.Client.Params;
using System.Diagnostics;
using System.Security.Cryptography;

namespace DecadeMobile;

public partial class MainPage : ContentPage
{
    const string Url = "https://iw233.cn/API/Random.php";
    readonly Dictionary<Task, Stream> tasks = new Dictionary<Task, Stream>();
    public MainPage()
	{
		InitializeComponent();
	}

	private void OnCounterClicked(object sender, EventArgs e)
    {
        MainPanel.Children.Clear();
        CounterBtn.Text = "请等待...";
        foreach(var kv in tasks)
        {
            if (kv.Key != null)
            {
                if (!kv.Key.IsCompleted)
                {
                    return;
                }
            }
        }
        抽();
	}
    public void 抽()
    {
        new Task(() =>
        {
            foreach (var kv in tasks)
            {
                if (kv.Key != null)
                {
                    kv.Value?.Close();
                }
            }
            tasks.Clear();
            for (var i = 0; i < 10; i++)
            {
                CancellationTokenSource cancel = new();
                Task task = null;
                task = new Task(() =>
                {
                    var hs = GetImageFromResponse(Url);
                    tasks[task] = hs;
                    var Image = GetImageFromBytes(hs);
                    Dispatcher.Dispatch(new Action(() =>
                    {
                        MainPanel.Add(Image);
                    }));
                },cancel.Token);
                tasks.Add(task, null);
                var watch = new Task(() => 
                { 
                    Thread.Sleep(5000);
                    if (!task.IsCompleted) { 
                        cancel.Cancel(); 
                    } 
                });
                task.Start();
            }
            new Task(() => {
                Task.WaitAll(tasks.Keys.ToArray());
                Dispatcher.Dispatch(new Action(() =>
                {
                    CounterBtn.Text = "重来！";
                }));    
            }).Start();
        }).Start();
    }
    private static ImageButton GetImageFromBytes(Stream stream)
    {
        ImageButton img = new () { 
            Source = ImageSource.FromStream(() => { return stream; }) ,
            Margin = 0,
            Padding = 0,
        };
        img.Clicked += (o,e) =>
        {
            Console.WriteLine("???");
        };
        return img;

    }
    public static Stream GetImageFromResponse(string url)
    {
        try
        {
            HttpClient client = new HttpClient();
            client.MaxResponseContentBufferSize = 1024*1024*20;
            var task = client.GetAsync(url);
            task.Wait();
            var response = task.Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStream();
                return content;
            }
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Debugger.Break();
        }
        return null;
    }
}

