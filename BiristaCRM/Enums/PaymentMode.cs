namespace BiristaCRM.Enums
{
    public enum PaymentMode
    {
        Cash = 0,
        Online = 1,
        Credit = 2,
        Cheque = 3
    }
    public enum PaymentStatus
    {
        Pending = 1,
        Partial = 2,
        Paid = 3,
        Cancelled = 4
    }
    //Pending → Payment haju malyu nathi.
    //Partial → Thodu payment malyu che, baki che.
    //Paid → Full payment mali gayu che.
    //Cancelled → Order cancel thai gayo.
}
