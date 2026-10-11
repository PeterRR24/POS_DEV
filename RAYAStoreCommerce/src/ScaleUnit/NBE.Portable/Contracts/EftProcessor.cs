using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RR.Dynamics.Retail.Payment.NBE.Contract
{
    // Processor capabilities
    public abstract class EftProcessorDescriptor
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public virtual bool IsOperationSupported(EftOperationType operation) { return false; }
        public virtual bool IsGiftCardProcessor { get { return true; } }
        public virtual int NeedProductDescription { get { return 0; } }
        public virtual string CreditCardTypes { get { return null; } }
        public virtual string CreditCardCurrencies(string creditCardType) { return null; }
        public virtual bool IsTokenListSupported { get { return false; } }
        public virtual bool HasUserInterface { get { return false; } }
    }

    public interface IEftProcessor 
    {
        EftService Service { get; }

        EftResponse Process(EftRequest request);
     
    }
 
}
