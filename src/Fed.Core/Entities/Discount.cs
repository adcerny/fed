using System;
using System.Collections.Generic;
using System.Linq;
using Fed.Core.Common.Interfaces;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.ValueTypes;

namespace Fed.Core.Entities
{
    public class Discount
    {
        public Discount()
        { }


        public Discount(
        Guid id,
        string name,
        string description,
        DiscountRewardType rewardType,
        DiscountQualificationType qualificationType,
        DiscountEligibleProductsType eligibleProductsType,
        decimal? percentage,
        decimal? value,
        decimal? minOrderValue,
        decimal? maxOrderValue,
        bool isInactive,
        bool isExclusive,
        DiscountEvent appliedEvent,
        Date appliedStartDate,
        Date? appliedEndDate,
        DiscountEvent startEvent,
        Date? startEventEndDate,
        int? periodFromStartDays,
        IList<Guid> eligibleProductCategoryIds,
        IList<Guid> qualifyingProductCategoryIds,
        IList<DiscountCode> discountCodes,
        IList<LineItem> discountedProducts,
        IList<LineItem> qualifyingProducts,
        IList<string> eligibleProductCategorySkus = null,
        IList<DiscountQualifyingCategory> qualifyingCategories = null
        )

        {
            Id = id;
            Name = name;
            Description = description;
            RewardType = rewardType;
            QualificationType = qualificationType;
            EligibleProductsType = eligibleProductsType;
            Percentage = percentage;
            Value = value;
            MinOrderValue = minOrderValue;
            MaxOrderValue = maxOrderValue;
            IsInactive = isInactive;
            IsExclusive = isExclusive;
            AppliedEvent = appliedEvent;
            AppliedStartDate = appliedStartDate;
            AppliedEndDate = appliedEndDate;
            StartEvent = startEvent;
            StartEventEndDate = startEventEndDate;
            PeriodFromStartDays = periodFromStartDays;
            DiscountCodes = discountCodes;
            DiscountedProducts = discountedProducts;
            QualifyingProducts = qualifyingProducts;
            EligibleProductCategoryIds = eligibleProductCategoryIds;
            EligibleProductCategorySkus = eligibleProductCategorySkus;
            QualifyingProductCategories = qualifyingCategories;
        }


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Value { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxOrderValue { get; set; }
        public bool IsInactive { get; set; }
        public bool IsExclusive { get; set; }
        public DiscountEvent AppliedEvent { get; set; }
        public Date AppliedStartDate { get; set; }
        public Date? AppliedEndDate { get; set; }
        public DiscountEvent StartEvent { get; set; }
        public Date? StartEventEndDate { get; set; }
        public int? PeriodFromStartDays { get; set; }
        public IList<Guid> EligibleProductCategoryIds { get; set; }
        public IList<string> EligibleProductCategorySkus { get; set; }
        public IList<DiscountQualifyingCategory> QualifyingProductCategories { get; set; }
        public IList<DiscountCode> DiscountCodes { get; set; }
        public IList<LineItem> DiscountedProducts { get; set; }
        public IList<LineItem> QualifyingProducts { get; set; }
        public DiscountRewardType RewardType { get; set; }
        public DiscountQualificationType QualificationType { get; set; }
        public DiscountEligibleProductsType EligibleProductsType { get; set; }

        public decimal GetPercentageDiscount(decimal total) => Percentage.HasValue ? 
            Math.Round((total * (Percentage.Value / 100)), 2, MidpointRounding.AwayFromZero) : total;
    }
}
