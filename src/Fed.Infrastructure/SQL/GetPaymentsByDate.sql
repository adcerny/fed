SELECT
	d.ShortId AS DeliveryId,
	d.DeliveryCompanyName as CompanyName,
	ct.AmountRequested,
	ct.AmountCaptured,
	ct.ResponseText,
	ct.TimeCreated AS PaymentTime
FROM CardTransactions AS ct
INNER JOIN Deliveries AS d ON ct.DeliveryId = d.Id
WHERE 
	CAST(ct.TimeCreated AS date) >= @FromDate
	AND CAST(ct.TimeCreated AS date) <= @ToDate
ORDER BY
	ct.TimeCreated DESC,
	d.DeliveryCompanyName