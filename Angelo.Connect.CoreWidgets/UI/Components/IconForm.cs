﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Angelo.Connect.CoreWidgets.Services;
using Angelo.Connect.CoreWidgets.Models;

namespace Angelo.Connect.CoreWidgets.UI.Components
{
    public class IconForm : ViewComponent
    {
        private IconService _IconService;

        public IconForm(IconService IconService)
        {
            _IconService = IconService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Icon model)
        {
            return await Task.Run(() =>  View(model));
        }
    }
}
