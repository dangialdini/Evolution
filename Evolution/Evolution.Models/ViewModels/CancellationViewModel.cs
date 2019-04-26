using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class CancellationViewModel : ViewModelBase {
        public CancellationStep1 Step1 { set; get; } = new CancellationStep1();
        public CancellationStep2 Step2 { set; get; } = new CancellationStep2();
        public CancellationStep3 Step3 { set; get; } = new CancellationStep3();
        public CancellationStep4 Step4 { set; get; } = new CancellationStep4();
        public CancellationStep5 Step5 { set; get; } = new CancellationStep5();
        public CancellationStep6 Step6 { set; get; } = new CancellationStep6();
        public CancellationStep7 Step7 { set; get; } = new CancellationStep7();
    }
}
