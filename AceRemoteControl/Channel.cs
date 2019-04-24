using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace AceRemoteControl
{
    public class Channel : BindableBase
    {
        private bool _isSelected;
        private string _text;
        private int _positionNumber;
        private string _audioChannelName;
        private string _type;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        public int PositionNumber
        {
            get { return _positionNumber; }
            set { SetProperty(ref _positionNumber, value); }
        }

        public string AudioChannelName
        {
            get => _audioChannelName;
            set => SetProperty(ref _audioChannelName, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public Channel Clone()
        {
            return (Channel) MemberwiseClone();
        }
    }
}
