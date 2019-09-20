using Emgu.CV; // Contains Mat and CvInvoke classes
using Emgu.CV.CvEnum;
using ScottPlot;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

internal struct Channels
{
    public double red;
    public double green;
    public double blue;

    public Channels(double r, double g, double b)
    {
        red = r;
        green = g;
        blue = b;
    }
}

class Program
{
    static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        Dispatch_args(args);
    }

    static void Dispatch_args(string[] args)
    {
        if (args.Length != 3) 
        {
            //qtde de argumentos invalidos
            Console.WriteLine("Modo de usar: EmguApp.exe --v nome_do_video nome_do_grafico");
            Environment.Exit(0);
        }
        else if (args[0] == "--v")
        {
            var Processor = new VideoProcessor(args[1], args[2]);
            Processor.ProcessVideo();
            Processor.savePlot(args[2]);
       }
       else
        {
            //Formato de argumento invalido
            Console.WriteLine("Modo de usar: EmguApp.exe --v nome_do_video nome_do_grafico");
            Environment.Exit(1);
        }
    }
}


class VideoProcessor : Form
{

    private List<Channels> channels = new List<Channels>();
    private string FilePath { get; set; }
    private string GraphPath { get; set; }
    private ScottPlot.FormsPlot Plotter;

    public VideoProcessor(string path, string graph)
    {
        this.FilePath = path;
        this.GraphPath = graph;
        this.InitializeComponent();
    }


    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }


    #region Windows Form Designer generated code    
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoProcessor));
        this.Plotter = new ScottPlot.FormsPlot();
        this.SuspendLayout();
        // 
        // formsPlot1
        // 
        this.Plotter.BackColor = System.Drawing.Color.White;
        this.Plotter.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("formsPlot1.BackgroundImage")));
        this.Plotter.Dock = System.Windows.Forms.DockStyle.Fill;
        this.Plotter.Location = new System.Drawing.Point(0, 0);
        this.Plotter.Name = "formsPlot1";
        this.Plotter.Size = new System.Drawing.Size(550, 286);
        this.Plotter.TabIndex = 0;
        // 
        // Form1
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(550, 286);
        this.Controls.Add(this.Plotter);
        this.Name = "Form1";
        this.Text = "ScottPlot Quickstart (Forms)";
        this.ResumeLayout(false);

    }

#endregion

    public void ProcessVideo()
    {
        using (var capture = new VideoCapture(this.FilePath)) // Loading video from file
        {
            if (capture.IsOpened)
            {
                var frame = capture.QueryFrame();

                VideoProcessingLoop(capture, frame);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
    }
    private void VideoProcessingLoop(VideoCapture a, Mat b)
    {
        while (true)
        {
            var frame = a.QueryFrame();
            if (frame != null)
            {
                Channels pixel = this.PixelAverage(frame.ToImage<Bgr, byte>());
                this.channels.Add(pixel);

                //CvInvoke.Imshow("VideoCapture", frame);
                this.Plot();
                //CvInvoke.Imshow("Graph", this.Plot());
                //CvInvoke.WaitKey(delay:1); // Render image and keep window opened until any key is pressed
            }
            else
            {
                Console.WriteLine("Processamento finalizado");
                return;
            }
        }
    }

    private Channels PixelAverage(Image<Bgr, byte> image)
    {
        int width = image.Cols;
        int height = image.Rows;
        int size = width * height;

        //definir o valor dos canais
        double red = 0;
        double green = 0;
        double blue = 0;

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                blue += (double)Convert.ToInt32(image.Data[i, j, 0]) / size;
                green += (double)Convert.ToInt32(image.Data[i, j, 1]) / size;
                red += (double)Convert.ToInt32(image.Data[i, j, 2]) / size;

            }
        }
        return new Channels(red, green, blue);
    }

    private void Plot()
    {
        const int framerate = 30;
        int pointCount = this.channels.Count;
        var dataXs = ScottPlot.DataGen.Consecutive(pointCount).Select((Point)=> Point/framerate).ToArray();
        double[] red = new double[pointCount];
        double[] green = new double[pointCount];
        double[] blue = new double[pointCount];

        this.Plotter.plt.Axis(x1: 0, y1: 0, y2: 255, x2: pointCount / framerate);

        for (int i = 0; i < pointCount; i++)
        {
            red[i] = this.channels[i].red;
            green[i] = this.channels[i].green;
            blue[i] = this.channels[i].blue;
            this.Plotter.plt.PlotScatter(dataXs, red, color: Color.Red);
            this.Plotter.plt.PlotScatter(dataXs, green, color: Color.Green);
            this.Plotter.plt.PlotScatter(dataXs, blue, color: Color.Blue);
            this.Plotter.Render();
        }


        //Image<Bgr, byte> img = new Image<Bgr, byte>(this.plotter.GetBitmap(renderFirst:true, lowQuality:false));
        //CvInvoke.Imshow("Plot", img.Mat);
        //CvInvoke.WaitKey();

    }

    public void savePlot(string filename)
    {
        this.Plot();
        string path = $"plots/{filename}.jpg";
        //this.plotter.SaveFig(path);
        Console.WriteLine($"Gráfico salvo com sucesso em {path}");
    }

}