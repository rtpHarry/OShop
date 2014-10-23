﻿using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Environment.Extensions;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Mvc.Extensions;
using Orchard.Security;
using Orchard.Services;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Utility.Extensions;
using OShop.Models;
using OShop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OShop.Controllers
{
    [OrchardFeature("OShop.Customers")]
    public class CustomerController : Controller, IUpdateModel
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IContentManager _contentManager;
        private readonly ITransactionManager _transactionManager;
        private readonly IClock _clock;
        private readonly ICustomersService _customersService;

        public CustomerController(
            IOrchardServices orchardServices,
            IAuthenticationService authenticationService,
            IContentManager contentManager,
            ITransactionManager transactionManager,
            IClock clock,
            ICustomersService customersService
            ) {
            Services = orchardServices;
            _authenticationService = authenticationService;
            _contentManager = contentManager;
            _transactionManager = transactionManager;
            _clock = clock;
            _customersService = customersService;
        }

        public IOrchardServices Services { get; private set; }
        public Localizer T { get; set; }

        [Themed]
        public ActionResult Index()
        {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Index", "Customer", new { area = "OShop" }) });
            }
            
            var customer = _customersService.GetCustomer(user.Id);
            if (customer != null) {
                return new ShapeResult(this, _contentManager.BuildDisplay(customer.ContentItem));
            }
            else {
                return RedirectToAction("Create");
            }
        }

        [HttpPost, ActionName("Index")]
        [FormValueRequired("Action")]
        public ActionResult IndexPost(string Action, int CustomerAddressId) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Index", "Customer", new { area = "OShop" }) });
            }

            switch (Action) {
                case "Edit":
                    return RedirectToAction("EditAddress", "Customer", new { area = "OShop", id = CustomerAddressId, ReturnUrl = Url.Action("Index", "Customer", new { area = "OShop" }) });
                case "Remove":
                    return RedirectToAction("RemoveAddress", "Customer", new { area = "OShop", id = CustomerAddressId, ReturnUrl = Url.Action("Index", "Customer", new { area = "OShop" }) });
                default :
                    return Index();
            }
        }

        [Themed]
        public ActionResult Create(string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var customer = _customersService.GetCustomer(user.Id);
            if (customer == null) {
                return View(_contentManager.BuildEditor(_contentManager.New("Customer")));
            }
            else {
                return RedirectToAction("Edit");
            }
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Save")]
        public ActionResult CreatePost(string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var customer = _contentManager.New("Customer");

            _contentManager.Create(customer, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(customer, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(customer);

            Services.Notifier.Information(T("Your customer account was successfully created."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [Themed]
        public ActionResult Edit(string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Edit", "Customer", new { area = "OShop" }) });
            }

            var customer = _customersService.GetCustomer(user.Id);
            if (customer != null) {
                return View(_contentManager.BuildEditor(customer.ContentItem));
            }
            else {
                return RedirectToAction("Create");
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPost(string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var customerPart = _customersService.GetCustomer(user.Id);

            var customer = _contentManager.Get(customerPart.ContentItem.Id, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(customer, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(customer);

            Services.Notifier.Information(T("Your customer account was successfully updated."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [Themed]
        public ActionResult CreateAddress(string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("CreateAddress", "Customer", new { area = "OShop" }) });
            }

            return View(_contentManager.BuildEditor(_contentManager.New("CustomerAddress")));
        }

        [Themed]
        [HttpPost, ActionName("CreateAddress")]
        [FormValueRequired("submit.Save")]
        public ActionResult CreateAddressPost(bool IsDefaultAddress = false, string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var customerAddress = _contentManager.New("CustomerAddress");

            _contentManager.Create(customerAddress, VersionOptions.Draft);
            var model = _contentManager.UpdateEditor(customerAddress, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(customerAddress);

            if (IsDefaultAddress) {
                SetDefaultAddress(customerAddress);
            }

            Services.Notifier.Information(T("Your customer account was successfully created."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        [Themed]
        public ActionResult EditAddress(int id, string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Edit", "Customer", new { area = "OShop" }) });
            }

            var address = _contentManager.Get<CustomerAddressPart>(id, VersionOptions.Latest);
            if (address == null) {
                return HttpNotFound();
            }

            if (address.Owner == null || address.Owner.Id != user.Id) {
                return new HttpUnauthorizedResult();
            }

            return View(_contentManager.BuildEditor(address));
        }

        [Themed]
        [HttpPost, ActionName("EditAddress")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditAddressPost(int id, string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var address = _contentManager.Get<CustomerAddressPart>(id, VersionOptions.Latest);
            if (address == null) {
                return HttpNotFound();
            }

            if (address.Owner == null || address.Owner.Id != user.Id) {
                return new HttpUnauthorizedResult();
            }

            var model = _contentManager.UpdateEditor(address.ContentItem, this);

            if (!ModelState.IsValid) {
                _transactionManager.Cancel();
                return View(model);
            }

            _contentManager.Publish(address.ContentItem);

            Services.Notifier.Information(T("Your address was successfully updated."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        public ActionResult RemoveAddress(int id, string returnUrl = null) {
            var user = _authenticationService.GetAuthenticatedUser();
            if (user == null) {
                return RedirectToAction("LogOn", "Account", new { area = "Orchard.Users", ReturnUrl = Url.Action("Create", "Customer", new { area = "OShop" }) });
            }

            var address = _contentManager.Get<CustomerAddressPart>(id, VersionOptions.Latest);
            if (address == null) {
                return HttpNotFound();
            }

            if (address.Owner == null || address.Owner.Id != user.Id) {
                return new HttpUnauthorizedResult();
            }

            _contentManager.Remove(address.ContentItem);

            Services.Notifier.Information(T("Your address was successfully removed."));

            return this.RedirectLocal(returnUrl, () => RedirectToAction("Index"));
        }

        private void SetDefaultAddress(ContentItem customerAddress) {
            var customer = _customersService.GetCustomer();
            if (customer != null) {
                customer.DefaultAddressId = customerAddress.Id;
            }
        }

        void IUpdateModel.AddModelError(string key, Orchard.Localization.LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }
    }
}