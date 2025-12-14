using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService, IOrderService orderService)
        {
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
        }


        /// <summary>
        /// Returns an invoices matching a given contact Id.
        /// </summary>
        /// <param name="contactId">The contact Id.</param>
        [HttpGet("/invoices")]
        public async Task<ActionResult<IList<Invoice>>> GetInvoicesByContactId(Guid contactId)
        {
            var invoices = await _invoiceService.GetInvoices(contactId);
            return Ok(invoices);
        }

        /// <summary>
        /// Returns an invoice matching the supplied Id.
        /// </summary>
        /// <param name="id">The invoice Id.</param>
        /// <response code="404">Invoice not found</response>
        [HttpGet("/invoices/{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(Guid id)
        {
            var invoice = await _invoiceService.GetInvoice(id);

            if (invoice == null)
                return NotFound($"No invoice with an ID of {id} exists.");

            return Ok(invoice);
        }

        /// <summary>
        /// Returns an PDF file for a given invoice.
        /// </summary>
        /// <param name="id">The invoice Id.</param>
        /// <response code="404">Invoice not found</response>
        [HttpGet("/invoices/{id}/pdf")]
        public async Task<ActionResult> GetInvoicePdf(Guid id)
        {
            var pdfStream = await _invoiceService.GetInvoicePdf(id);
            return File(pdfStream, "application/pdf", $"{id}.pdf");
        }

        /// <summary>
        /// Creates an invoice
        /// </summary>
        [HttpPost("/invoices")]
        public async Task<ActionResult<Guid>> CreateInvoice(Invoice invoice)
        {
            var id = await _invoiceService.CreateInvoice(invoice);
            return Ok(id);
        }

        /// <summary>
        /// Gets a list of new invoices required for a given date range.
        /// </summary>
        /// <param name="fromDate">The first day of the invoice period.</param>
        /// <param name="toDate">The last day of the invoice period.</param>
        [HttpPut("/invoices")] //TODO - remove put
        [HttpGet("/invoices/required")]
        public async Task<ActionResult<IList<Guid>>> GetRequiredInvoices([FromQuery]DateTime fromDate, [FromQuery]DateTime toDate)
        {
            var invoices = await _invoiceService.GenerateInvoices(DateRange.Create(fromDate, toDate));
            return Ok(invoices);
        }
    }
}