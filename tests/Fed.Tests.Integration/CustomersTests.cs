using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Tests.Common;
using Fed.Web.Service.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class CustomersTests
    {
        [Fact]
        public async Task GetCustomers()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            var companyName1 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var companyName2 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var companyName3 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";

            var email1 = TestDataBuilder.GetRandomEmail();
            var email2 = TestDataBuilder.GetRandomEmail();
            var email3 = TestDataBuilder.GetRandomEmail();

            // Create some customers:
            var customer1 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName1,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                     {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Christina",
                            "Lanson",
                            email1,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "Please deliver to 5th floor.",
                                    "+44 (0) 74305 93022",
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "office@examle.org",
                                    "12345985",
                                    "PO12345")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Christina Lanson",
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "EC1V 2NX",
                                    true)
                            })
                     }
                };

            var customer2 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName2,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                    {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Oliver",
                            "R. Phillips",
                            email2,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "office@examle.org",
                                    "12345985",
                                    "PO12345")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Oliver R. Phillips",
                                    "133 Whitechapel High Street",
                                    "E1 7QA",
                                    true)
                            })
                    }
                };

            var customer3 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName3,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    true,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                 {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Wallace",
                            "Wilcox",
                            email3,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    "office@examle.org",
                                    "12345985",
                                    "PO12345")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Wallace Wilcox",
                                    "5 Devonshire Square",
                                    "EC2M 4YD",
                                    true)
                            })
                 }
                };

            customer1 = await fedClient.CreateCustomerAsync(customer1);
            customer2 = await fedClient.CreateCustomerAsync(customer2);
            customer3 = await fedClient.CreateCustomerAsync(customer3);

            var customers = await fedClient.GetCustomersAsync(false);

            Assert.Contains(customers, c => c.CompanyName.Equals(companyName1));
            Assert.Contains(customers, c => c.CompanyName.Equals(companyName2));
            Assert.Contains(customers, c => c.CompanyName.Equals(companyName3));

            Assert.NotNull(customers[0].Contacts);
            Assert.NotNull(customers[1].Contacts);
            Assert.NotNull(customers[2].Contacts);

            Assert.Empty(customers[0].Contacts);
            Assert.Empty(customers[1].Contacts);
            Assert.Empty(customers[2].Contacts);
        }

        [Fact]
        public async Task GetCustomersWithContacts()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            var companyName1 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var companyName2 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var companyName3 = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";

            var email1 = TestDataBuilder.GetRandomEmail();
            var email2 = TestDataBuilder.GetRandomEmail();
            var email3 = TestDataBuilder.GetRandomEmail();

            // Create some customers:
            var customer1 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName1,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                     {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Christina",
                            "Lanson",
                            email1,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "Please deliver to 5th floor.",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "office@examle.org",
                                    "12345985",
                                    "PO12345")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Christina Lanson",
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "EC1V 2NX",
                                    true)
                            })
                     }
                };

            var customer2 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName2,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                    {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Oliver",
                            "R. Phillips",
                            email2,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "office@examle.org",
                                    "12345985",
                                    "PO1234")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Oliver R. Phillips",
                                    "133 Whitechapel High Street",
                                    "E1 7QA",
                                    true)
                            })
                    }
                };

            var customer3 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName3,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                     {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Wallace",
                            "Wilcox",
                            email3,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    "office@examle.org",
                                    "12345985",
                                    "PO1234")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Wallace Wilcox",
                                    "5 Devonshire Square",
                                    "EC2M 4YD",
                                    true)
                            })
                     }
                };

            customer1 = await fedClient.CreateCustomerAsync(customer1);
            customer2 = await fedClient.CreateCustomerAsync(customer2);
            customer3 = await fedClient.CreateCustomerAsync(customer3);

            var customers = await fedClient.GetCustomersAsync(true);

            Assert.Contains(customers, c => c.CompanyName.Equals(companyName1));
            Assert.Contains(customers, c => c.CompanyName.Equals(companyName2));
            Assert.Contains(customers, c => c.CompanyName.Equals(companyName3));

            Assert.NotNull(customers[0].Contacts);
            Assert.NotNull(customers[1].Contacts);
            Assert.NotNull(customers[2].Contacts);

            Assert.NotEmpty(customers[0].Contacts);
            Assert.NotEmpty(customers[1].Contacts);
            Assert.NotEmpty(customers[2].Contacts);
        }

        [Fact]
        public async Task CustomerCRUD()
        {
            // POST /customers
            var fedClient = FedWebClient.Create(new NullLogger());

            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            var customerName = Guid.NewGuid().ToString("N").Substring(0, 10) + " Ltd";
            var email = TestDataBuilder.GetRandomEmail();
            var newCustomer =
                new Customer(
                    Guid.Empty,
                    "",
                    customerName,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null);

            var deliveryAddresses = new[]
                {
                    new DeliveryAddress(
                        Guid.Empty,
                        Guid.Empty,
                        true,
                        "Peter Parker",
                        customerName,
                        "Park Avenue",
                        "",
                        "London",
                        "SW1 4RA",
                        "Don't ring the bell, just walk to reception",
                        Faker.Phone.Number(),
                        false,
                        hubId)
                };

            var billingAddresses = new[]
                {
                    new BillingAddress(
                        Guid.Empty,
                        Guid.Empty,
                        true,
                        "Peter Parker",
                        customerName,
                        "Park Avenue",
                        "",
                        "London",
                        "SW1 4RA",
                        "office@example.org",
                        "21654985",
                        "PO1234")
                };

            var cardTokens = new[]
            {
                new CardToken(
                    Guid.Empty,
                    Guid.Empty,
                    10,
                    2020,
                    "2345 **** **** ****",
                    "PETER PARKER",
                    "Park Avenue",
                    "SW1 4RA",
                    true)
            };

            newCustomer.Contacts =
                new[]
                {
                    new Contact(
                        Guid.Empty,
                        "",
                        Guid.Empty,
                        "Mr.",
                        "Peter",
                        "Parker",
                        email,
                        "123456789",
                        false,
                        false,
                        1,
                        DateTime.UtcNow,
                        deliveryAddresses,
                        billingAddresses,
                        cardTokens)
                };

            var customer = await fedClient.CreateCustomerAsync(newCustomer);

            Assert.Equal(customerName, customer.CompanyName);
            Assert.Equal(customerName, customer.Contacts[0].DeliveryAddresses[0].CompanyName);
            Assert.Equal(customerName, customer.Contacts[0].BillingAddresses[0].CompanyName);

            Assert.Equal(email, customer.Contacts[0].Email);
            Assert.Equal("123456789", customer.Contacts[0].Phone);
            Assert.False(customer.Contacts[0].IsMarketingConsented);

            Assert.Equal("Peter", customer.Contacts[0].FirstName);
            Assert.Equal("Parker", customer.Contacts[0].LastName);
            Assert.Equal("Peter Parker", customer.Contacts[0].DeliveryAddresses[0].FullName);
            Assert.Equal("Peter Parker", customer.Contacts[0].BillingAddresses[0].FullName);

            Assert.Equal("Park Avenue", customer.Contacts[0].DeliveryAddresses[0].AddressLine1);
            Assert.Equal("Park Avenue", customer.Contacts[0].BillingAddresses[0].AddressLine1);
            Assert.Equal("", customer.Contacts[0].DeliveryAddresses[0].AddressLine2);
            Assert.Equal("", customer.Contacts[0].BillingAddresses[0].AddressLine2);
            Assert.Equal("London", customer.Contacts[0].DeliveryAddresses[0].Town);
            Assert.Equal("London", customer.Contacts[0].BillingAddresses[0].Town);
            Assert.Equal("SW1 4RA", customer.Contacts[0].DeliveryAddresses[0].Postcode);
            Assert.Equal("SW1 4RA", customer.Contacts[0].BillingAddresses[0].Postcode);

            Assert.True(customer.Contacts[0].DeliveryAddresses[0].IsPrimary);
            Assert.True(customer.Contacts[0].BillingAddresses[0].IsPrimary);
            Assert.True(customer.Contacts[0].CardTokens[0].IsPrimary);

            Assert.Equal("Don't ring the bell, just walk to reception", customer.Contacts[0].DeliveryAddresses[0].DeliveryInstructions);
            Assert.Equal(hubId, customer.Contacts[0].DeliveryAddresses[0].HubId);
            Assert.False(customer.Contacts[0].DeliveryAddresses[0].LeaveDeliveryOutside);

            Assert.Equal(10, customer.Contacts[0].CardTokens[0].ExpiresMonth);
            Assert.Equal(2020, customer.Contacts[0].CardTokens[0].ExpiresYear);
            Assert.Equal("PETER PARKER", customer.Contacts[0].CardTokens[0].CardHolderFullName);
            Assert.Equal("2345 **** **** ****", customer.Contacts[0].CardTokens[0].ObscuredCardNumber);

            // PATCH /customers/{id}

            var patchedName = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchCompanyName = PatchOperation.CreateReplace("/companyName", patchedName);
            await fedClient.PatchCustomerAsync(customer.Id, patchCompanyName);

            // PATCH /customers/{customerId}/contacts/{contactId}

            var patchedName2 = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchCustomerFirstName = PatchOperation.CreateReplace("/firstName", patchedName2);
            await fedClient.PatchContactAsync(customer.Id, customer.Contacts[0].Id, patchCustomerFirstName);

            // PATCH /customers/{customerId}/contacts/{contactId}/deliveryAddresses/{deliveryAddressId}

            var patchedName3 = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchDeliveryAddressFullName = PatchOperation.CreateReplace("/fullName", patchedName3);
            await fedClient.PatchDeliveryAddressAsync(customer.Id, customer.Contacts[0].Id, customer.Contacts[0].DeliveryAddresses[0].Id, patchDeliveryAddressFullName);

            // PATCH /customers/{customerId}/contacts/{contactId}/billingAddresses/{billingAddressId}

            var patchedName4 = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchBillingAddressFullName = PatchOperation.CreateReplace("/fullName", patchedName4);
            await fedClient.PatchBillingAddressAsync(customer.Id, customer.Contacts[0].Id, customer.Contacts[0].BillingAddresses[0].Id, patchBillingAddressFullName);

            var patchedName5 = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchCustomerLastName = PatchOperation.CreateReplace("/lastName", patchedName5);
            await fedClient.PatchContactAsync(customer.Id, customer.Contacts[0].Id, patchCustomerLastName);


            // GET /customers/{id}
            var customer2 = await fedClient.GetCustomerAsync(customer.Id);

            Assert.Equal(patchedName, customer2.CompanyName);
            Assert.Equal(patchedName2, customer2.Contacts[0].FirstName);
            Assert.Equal(patchedName5, customer2.Contacts[0].LastName);
            Assert.Equal(customer.Contacts[0].Email, customer2.Contacts[0].Email);
            Assert.Equal(customer.Contacts[0].Phone, customer2.Contacts[0].Phone);
            Assert.Equal(customer.Contacts[0].IsMarketingConsented, customer2.Contacts[0].IsMarketingConsented);
            Assert.Equal(customer.Contacts[0].PaymentMethod, customer2.Contacts[0].PaymentMethod);

            Assert.Equal(patchedName3, customer2.Contacts[0].DeliveryAddresses[0].FullName);
            Assert.Equal(customer.Contacts[0].DeliveryAddresses[0].AddressLine1, customer2.Contacts[0].DeliveryAddresses[0].AddressLine1);
            Assert.Equal(customer.Contacts[0].DeliveryAddresses[0].AddressLine2, customer2.Contacts[0].DeliveryAddresses[0].AddressLine2);
            Assert.Equal(customer.Contacts[0].DeliveryAddresses[0].Town, customer2.Contacts[0].DeliveryAddresses[0].Town);
            Assert.Equal(customer.Contacts[0].DeliveryAddresses[0].Postcode, customer2.Contacts[0].DeliveryAddresses[0].Postcode);

            Assert.Equal(patchedName4, customer2.Contacts[0].BillingAddresses[0].FullName);
            Assert.Equal(customer.Contacts[0].BillingAddresses[0].AddressLine1, customer2.Contacts[0].BillingAddresses[0].AddressLine1);
            Assert.Equal(customer.Contacts[0].BillingAddresses[0].AddressLine2, customer2.Contacts[0].BillingAddresses[0].AddressLine2);
            Assert.Equal(customer.Contacts[0].BillingAddresses[0].Town, customer2.Contacts[0].BillingAddresses[0].Town);
            Assert.Equal(customer.Contacts[0].BillingAddresses[0].Postcode, customer2.Contacts[0].BillingAddresses[0].Postcode);

            Assert.Equal(customer.Contacts[0].CardTokens[0].ExpiresMonth, customer2.Contacts[0].CardTokens[0].ExpiresMonth);
            Assert.Equal(customer.Contacts[0].CardTokens[0].ExpiresYear, customer2.Contacts[0].CardTokens[0].ExpiresYear);
            Assert.Equal(customer.Contacts[0].CardTokens[0].CardHolderFullName, customer2.Contacts[0].CardTokens[0].CardHolderFullName);
            Assert.Equal(customer.Contacts[0].CardTokens[0].ObscuredCardNumber, customer2.Contacts[0].CardTokens[0].ObscuredCardNumber);
        }

        [Fact]
        public async Task GetCustomer()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            // Create some customers:
            var companyName = $"Integration-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var email = TestDataBuilder.GetRandomEmail();
            var customer1 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                    {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Christina",
                            "Lanson",
                            email,
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "Please deliver to 5th floor.",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "office@examle.org",
                                    "12345985",
                                    "PO1234")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Christina Lanson",
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "EC1V 2NX",
                                    true)
                            })
                    }
                };

            var result = await fedClient.CreateCustomerAsync(customer1);

            var contact = result.Contacts.First();
            var da = contact.GetPrimaryDeliveryAddress();
            var ct = contact.CardTokens.First();

            Assert.Equal(companyName, result.CompanyName);
            Assert.Equal("Christina", contact.FirstName);
            Assert.Equal("Lanson", contact.LastName);
            Assert.Equal(email, contact.Email);
            Assert.Equal("+44 (0) 74305 93021", contact.Phone);
            Assert.False(contact.IsMarketingConsented);
            Assert.Equal(1, (int)contact.PaymentMethod);
            Assert.Equal(companyName, result.CompanyName);
            Assert.Equal("Christina Lanson", da.FullName);
            Assert.Equal("Ramon Lee & Partners Kemp House, Office, City Rd", da.AddressLine1);
            Assert.Equal("5th Floor", da.AddressLine2);
            Assert.Equal("London", da.Town);
            Assert.Equal("EC1V 2NX", da.Postcode);
            Assert.Equal("Please deliver to 5th floor.", da.DeliveryInstructions);
            Assert.Equal("Christina Lanson", ct.CardHolderFullName);
            Assert.Equal("2345 **** **** ****", ct.ObscuredCardNumber);
            Assert.Equal(2020, ct.ExpiresYear);
            Assert.Equal(10, ct.ExpiresMonth);
            Assert.True(ct.IsPrimary);
        }
    }
}
