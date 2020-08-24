using System;
using System.Collections.Generic;
using Gtk;
using Pango;
using TestGTK;

public partial class MainWindow : Window
{
    IEnumerator<int> compiler = null;
    LineTracer tracer = null;
    uint? timer = null;
    bool autoStep = false;

    public MainWindow() : base(WindowType.Toplevel)
    {
        Build();
        Reset();
        FontDescription font = new FontDescription();
        font.Family = "monospace 20";
        lOutput.ModifyFont(font);
        eInput.SelectRegion(0, 0);
        eInput.ModifyFont(font);
        timer = GLib.Timeout.Add(10, () =>
        {
            if (autoStep)
                Step();
            return true;
        });
    }

    String Input { get { return eInput.Text; } }
    String Output { set { lOutput.Text = value; } }
    bool Active {
        set
        {
            eInput.IsEditable = value;
            eInput.Sensitive = value;
        }
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        Application.Quit();
        a.RetVal = true;
    }

    void Step()
    {
        Active = false;
        try
        {
            if (compiler == null)
            {
                compiler = Parser.Parse(Input).GetEnumerator();
                tracer = new LineTracer();
            }

            if (compiler.MoveNext())
            {
                int pc = compiler.Current;
                String command = Input[pc].ToString();
                if (!autoStep)
                    Output = SourceWithPc(pc);
                if (tracer.Step(command, sbStep.Value, Math.PI * sbAng.Value / 180))
                    DrawPath();
            }
            else
            {
                Reset();
            }
        }
        catch (Parser.SyntaxException exception)
        {
            Reset(exception.Message + "\n" + SourceWithPc(exception.Pc));
        }
    }

    void DrawPath()
    {
        Cairo.Context cr = Gdk.CairoHelper.Create(daCanvas.GdkWindow);

        if (tracer != null)
            tracer.DrawLine(cr, Allocation);

        cr.GetTarget().Dispose();
        cr.Dispose();
    }

    String SourceWithPc(int pc)
    {
        return Input + "\n" + new String(' ', pc) + "|";
    }

    void Reset(String message = "Ready")
    {
        autoStep = false;
        compiler = null;
        Output = message;
        Active = true;
        tracer = null;
    }

    protected void StepClicked(object sender, EventArgs e)
    {
        Step();
    }

    protected void StopClicked(object sender, EventArgs e)
    {
        Reset();
    }

    protected void FastForwardClicked(object sender, EventArgs e)
    {
        autoStep = !autoStep;
    }
}
