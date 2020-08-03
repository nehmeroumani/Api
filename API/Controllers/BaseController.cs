using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Core;
using Core.Cache;
using Core.Extentions;
using Core.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected Pool P = Pool.I;

        protected void T(int total, RequestData rd)
        {
            Response.Headers.Add("Content-Range", $"records {rd.Page}-{rd.Page * rd.Size}/" + total);
        }
        protected bool IsAdmin => User.IsInRole(((int)RoleEnum.Admin).ToString());

        //protected int UserId()
        //{
        //    return 0; //int.Parse(Request.Query["userid"]);
        //}

        protected BadRequestObjectResult Error(string message)
        {
            return BadRequest(new { message = message });
        }

        protected RequestData Rd()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var rd = new RequestData();
            try
            {
                foreach (var key in Request.Query.Keys)
                {
                    if (key == "cache")
                        rd.Cache = true;
                    else if (key == "page")
                        rd.Page = Convert.ToInt32(Request.Query[key]);
                    else if (key == "size")
                        rd.Size = Convert.ToInt32(Request.Query[key]);
                    else if (key == "order")
                        rd.Order = Request.Query[key];
                    else if (key == "sort")
                        rd.Sort = Request.Query[key];

                    else if (key == "filter")
                    {
                        var filters = Request.Query[key];
                        foreach (var f in filters)
                        {

                            var item = f.Split(',');
                            if (item.Length == 3 && item[2] != "null" && item[2] != "" && item[2] != "undefined")
                            {
                                string keyValue = item[0];
                                string operatorValue = item[1];
                                string valueData = item[2];
                                if (keyValue.ToLower().EndsWith("id"))
                                    operatorValue = "eq";
                                if (keyValue.ToLower().EndsWith("_datefilter"))
                                {
                                    var d = keyValue.Split("_");
                                    if (d.Length == 3)
                                    {
                                        operatorValue = d[0];
                                        keyValue = d[1];
                                    }

                                }
                                else if (keyValue.ToLower().EndsWith("_cutfilter"))
                                {
                                    var d = keyValue.Split("_");
                                    if (d.Length == 2)
                                    {
                                        keyValue = d[0];
                                    }
                                    var b = valueData.Split("_");
                                    if (b.Length == 2)
                                    {
                                        operatorValue = b[0];
                                        valueData = b[1];
                                    }
                                }

                                if (f.StartsWith("custom"))
                                    rd.CustomFilter.Add(new FilterData { Key = keyValue, Operator = operatorValue, Value = valueData });
                                else
                                    rd.Filter.Add(new FilterData { Key = keyValue, Operator = operatorValue, Value = valueData });
                            }
                        }
                    }
                    else if (key == "id" && !string.IsNullOrEmpty(Request.Query[key]))
                    {
                        rd.Ids = Request.Query[key].ToString().Split(',').Select(int.Parse).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return rd;
        }

    }
}
