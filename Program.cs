using System.ComponentModel;

public class TempreatureEventArgs : EventArgs {
    public DateTime dateTime {get;set;}
    public double info { get; set;}
}

public interface IDevice
{
    void runDevice();
    void handleEmergency();

    double warningTemp { get; }
    double EmergencyTemp { get; }
}

public class Device : IDevice
{

    const double warningLevelTemp = 27;
    const double emergencyLevelTemp = 85;

    public double warningTemp => warningLevelTemp;

    public double EmergencyTemp => emergencyLevelTemp;

    


    public void handleEmergency()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine("Encountered Emergency...\nShutting down system");
        shutdown();
        Console.ResetColor();
    }

    private void shutdown()
    {
        Console.WriteLine("Shutting down the system.");
    }

    public void runDevice()
    {
        Console.WriteLine("Device starting.");
        ICoolingDevice coolingDevice = new CoolingDevice();
        IHeatSensor heatSensor = new HeatSensor(warningTemp,EmergencyTemp);
        IThermostat thermostat = new thermostat(coolingDevice,heatSensor,this);
        thermostat.runTherm();

    }

    
}

public interface ICoolingDevice
{
    void turnOn();
    void turnOff();
}

public class CoolingDevice : ICoolingDevice
{
    public void turnOff()
    {
        Console.WriteLine("Cooling Device turned off.\n");
    }

    public void turnOn()
    {
        Console.WriteLine("Cooling Device turned on.\n");
    }
}

public interface IThermostat { 
     void runTherm();
}

public interface IHeatSensor
{
    event EventHandler<TempreatureEventArgs> TrempreatureReachesEmergencyLevelEventHandler;
    event EventHandler<TempreatureEventArgs> TrempreatureReachesWarningLevelEventHandler;
    event EventHandler<TempreatureEventArgs> TrempreatureFasllsBelowWarningLevelEventHandler;

    void RunSensor();

}

public class thermostat : IThermostat
{

    private ICoolingDevice _coolingDevice = null;
    private IHeatSensor _heatSensor = null;
    private IDevice _device = null;

    private const double warningLevelTemp = 27;
    private const double emergencyLevelTemp = 85;
    
    public thermostat(ICoolingDevice coolingDevice, IHeatSensor heatSensor, IDevice device)
    {
        _coolingDevice = coolingDevice;
        _heatSensor = heatSensor;
        _device = device;   
    }

    void WireUpToEvents()
    {
        _heatSensor.TrempreatureReachesWarningLevelEventHandler += _HeatSensor_TrempreatureReachesWarningLevelEventHandler;
        _heatSensor.TrempreatureReachesEmergencyLevelEventHandler += _HeatSensor_TrempreatureReachesEmergencyLevelEventHandler;
        _heatSensor.TrempreatureFasllsBelowWarningLevelEventHandler += _HeatSensor_TrempreatureFasllsBelowWarningLevelEventHandler;
    }

    private void _HeatSensor_TrempreatureFasllsBelowWarningLevelEventHandler(object? sender, TempreatureEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine();
        Console.WriteLine($"Information Alert !! Tempreature falls below warning level (warning level is between {_device.warningTemp}.");
        Console.ResetColor();
    }

    private void _HeatSensor_TrempreatureReachesEmergencyLevelEventHandler(object? sender, TempreatureEventArgs e)
    {
        Console.ForegroundColor= ConsoleColor.Red;
        Console.WriteLine();
        Console.WriteLine($"Emergeny Alert!! Tempreature reached Emergency level ( Emergency level is higher than {_device.EmergencyTemp})");
        Console.ResetColor();
    }

    private void _HeatSensor_TrempreatureReachesWarningLevelEventHandler(object? sender, TempreatureEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine();
        Console.WriteLine($"Warning Alert!! Tempreature reached warning level ( warning level is between {_device.warningTemp} and {_device.EmergencyTemp} )");
        Console.ResetColor();
    }

    void IThermostat.runTherm()
    {
        Console.WriteLine("Thermostat is running......");
        WireUpToEvents();
        _heatSensor.RunSensor();
    }
}



public class HeatSensor : IHeatSensor
{

    protected EventHandlerList _listEventDelegates = new EventHandlerList();

    static readonly object _tempreatureReachesWarningLevelKey = new object();
    static readonly object _tempreatureReachesEmergencyLevelKey = new object();
    static readonly object _tempreatureFallsBelowWarningLevelKey = new object();

    double _warningLevel = 0;
    double _emergencyLevel = 0;

    Boolean warningRaised = false;

    protected double[] _tempreatureData = null;


    public HeatSensor(double warningLevel, double emergencyLevel)
    {
        _warningLevel = warningLevel; 
        _emergencyLevel = emergencyLevel;

        seedTempData();
    }


    public void seedTempData()
    {
        _tempreatureData = new double[] {16,17,16.5,18,19,22,24,26.75,28.7,27.6,26,24,22,45,68,86.45 };
    }

    public void MontiorTempreature()
    {
        foreach(int t in _tempreatureData)
        {
            Console.WriteLine(t);
            if (t >= _warningLevel && t < _emergencyLevel)
            {
                warningRaised = true;
                TempreatureEventArgs e = new TempreatureEventArgs()
                {
                    dateTime = DateTime.Now,
                    info = t
                };

                OnTempreatureReachesWarningLevelHandler(e);
            }
            else if (t > _emergencyLevel)
            {
                warningRaised = true;
                TempreatureEventArgs e = new() { dateTime = DateTime.Now, info = t };
                OnTempreatureReachedEmergencyLevelHandler(e);
            }
            else if(t < _warningLevel && warningRaised)
            {
                warningRaised = false;
                TempreatureEventArgs e = new() { info = t , dateTime = DateTime.Now};
                OnTempreatureFallsBelowWarningLevelHandler(e);
            }
            Thread.Sleep(1000);
        }
    }

    private void OnTempreatureReachesWarningLevelHandler(TempreatureEventArgs e)
    {
        EventHandler<TempreatureEventArgs> handler = (EventHandler<TempreatureEventArgs>)_listEventDelegates[_tempreatureReachesWarningLevelKey];

        if(handler != null)
        {
            handler(this, e);
        }
    }


    private void OnTempreatureReachedEmergencyLevelHandler(TempreatureEventArgs e)
    {
        EventHandler<TempreatureEventArgs> handler = (EventHandler<TempreatureEventArgs>)_listEventDelegates[_tempreatureReachesEmergencyLevelKey];

        if (handler != null)
        {
            handler(this, e);
        }
    }

    private void OnTempreatureFallsBelowWarningLevelHandler(TempreatureEventArgs e)
    {
        EventHandler<TempreatureEventArgs> handler = (EventHandler<TempreatureEventArgs>)_listEventDelegates[_tempreatureFallsBelowWarningLevelKey];

        if (handler != null)
        {
            handler(this, e);
        }
    }

    event EventHandler<TempreatureEventArgs> IHeatSensor.TrempreatureReachesEmergencyLevelEventHandler
    {
        add
        {
            _listEventDelegates.AddHandler(_tempreatureReachesEmergencyLevelKey, value);
        }

        remove
        {
            _listEventDelegates.RemoveHandler(_tempreatureReachesEmergencyLevelKey, value);
        }
    }

    event EventHandler<TempreatureEventArgs> IHeatSensor.TrempreatureReachesWarningLevelEventHandler
    {
        add
        {
            _listEventDelegates.AddHandler(_tempreatureReachesWarningLevelKey, value);

        }

        remove
        {
            _listEventDelegates.RemoveHandler(_tempreatureReachesWarningLevelKey, value);
        }
    }

    event EventHandler<TempreatureEventArgs> IHeatSensor.TrempreatureFasllsBelowWarningLevelEventHandler
    {
        add
        {
            _listEventDelegates.AddHandler(_tempreatureFallsBelowWarningLevelKey, value);

        }

        remove
        {
            _listEventDelegates.RemoveHandler(_tempreatureFallsBelowWarningLevelKey, value);
        }
    }

    public void RunSensor()
    {
        Console.WriteLine("HeatSensor running");
        MontiorTempreature();
    }

}

public class program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Press any key to start system");
        Console.ReadLine();
        IDevice device = new Device();
        device.runDevice();
        Console.ReadLine();
    }
}
