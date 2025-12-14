using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Services.Common
{
    public abstract class DiscountStrategy : IDiscountCalculator
    {
        protected Discount _discount;
        protected IDiscountQualificationStrategy _qualificationStrategy;
        protected IDiscountEligibleProductsStrategy _eligibleProductsStrategy;

        public DiscountStrategy(Discount discount, 
                                IDiscountQualificationStrategy qualificationStrategy, 
                                IDiscountEligibleProductsStrategy eligibleProductsStrategy)
        {
            _discount = discount;
            _qualificationStrategy = qualificationStrategy;
            _eligibleProductsStrategy = eligibleProductsStrategy;
        }

        public abstract DiscountResult CalculateDiscount(IList<LineItem> items);

        public IList<LineItem> GetEligableProducts(IList<LineItem> items) => 
            _eligibleProductsStrategy.GetProducts(_discount, items);

        public DiscountQualification GetQualification(IList<LineItem> items) =>
            _qualificationStrategy.GetQualification(_discount, items);

        protected bool IsDiscountApplicableToCustomerDelivery(Customer customer, Date deliveryDate)
        {
            if (_discount.StartEvent == DiscountEvent.FirstOrder &&
               customer != null &&
               customer.LastDeliveryDate != null)
                return false;

            if (_discount.PeriodFromStartDays == null)
                return true;

            if (_discount.StartEvent == DiscountEvent.FirstOrder && customer.FirstDeliveryDate == null)
                return deliveryDate.Value < DateTime.Now.AddDays(_discount.PeriodFromStartDays.Value);

            if (_discount.StartEvent == DiscountEvent.FirstOrder)
                return deliveryDate.Value < (customer.FirstDeliveryDate ?? DateTime.Today).AddDays(_discount.PeriodFromStartDays.Value);

            //TODO - implement future logic for other start events
            return false;
        }
    }
}
