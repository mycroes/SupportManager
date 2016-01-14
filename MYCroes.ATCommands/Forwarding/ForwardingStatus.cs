namespace MYCroes.ATCommands.Forwarding
{
    public class ForwardingStatus
    {
        private bool active;
        private ForwardingClass @class;
        private string number;
        private ForwardingPhoneNumberType? numberType;
        private const string COMMAND = "+CCFC";

        public bool Active
        {
            get { return active; }
            set { active = value; }
        }

        public ForwardingClass Class
        {
            get { return @class; }
            set { @class = value; }
        }

        public string Number
        {
            get { return number; }
            set { number = value; }
        }

        public ForwardingPhoneNumberType? NumberType
        {
            get { return numberType; }
            set { numberType = value; }
        }

        public static ForwardingStatus Parse(string input)
        {
            var reader = new ATResponseValueReader(input);
            var forward = new ForwardingStatus();
            reader
                .Read(out forward.active)
                .Read(out forward.@class)
                .Read(out forward.number)
                .Read(out forward.numberType);

            return forward;
        }

        public static ATCommand Query(ForwardingReason reason)
        {
            return new ATCommand(COMMAND, reason, ForwardingMode.QueryStatus);
        }

        public static ATCommand Set(ForwardingReason reason, ForwardingMode mode, string phoneNumber, ForwardingPhoneNumberType phoneNumberType, ForwardingClass forwardingClass)
        {
            return new ATCommand(COMMAND, reason, mode, phoneNumber, phoneNumberType, forwardingClass);
        }
    }
}