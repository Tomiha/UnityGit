using System;

public class VenlyException : Exception
{
    public VenlyException(Exception ex) : base(ex.Message, ex)
    {
    }

    public VenlyException(string msg) : base(msg)
    {
    }
}
