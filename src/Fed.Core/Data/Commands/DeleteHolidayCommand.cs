using Fed.Core.ValueTypes;

namespace Fed.Core.Data.Commands
{
    public class DeleteHolidayCommand : IDataOperation<bool>
    {
        public DeleteHolidayCommand(Date date)
        {
            Date = date;
        }
        public Date Date { get; }
    }
}