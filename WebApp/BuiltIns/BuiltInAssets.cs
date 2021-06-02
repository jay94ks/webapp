using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Extensions;
using WebApp.Utils;

namespace WebApp.BuiltIns
{
    [Controller]
    public class BuiltInAssets : Controller
    {
        private static readonly DateTime RESP_ModifiedAt = DateTime.Now;
        private static readonly string RESP_ModifiedAt_String = RESP_ModifiedAt.ToString("r");
        private static readonly string RESP_ETag = string.Format("BA-{0}", Hash.MD5(RESP_ModifiedAt_String));

        private static byte[] RESP_jQuery = Encoding.UTF8.GetBytes(Resource.jquery_3_6_0_min);
        private static byte[] RESP_Vue = Encoding.UTF8.GetBytes(Resource.vue_2_6_12_min);

        /// <summary>
        /// Generate Built-in Asset Response.
        /// </summary>
        /// <returns></returns>
        public static IActionResult Provide(
            ControllerBase Controller, HttpRequest Request, HttpResponse Response,
            DateTime ModifiedAt, string ETag, ref byte[] Content, string Type)
        {
            string IfMatch = Request.Headers.GetHeader("If-Match");
            string IfNoneMatch = Request.Headers.GetHeader("If-None-Match");
            DateTime IfModifiedSince = Request.Headers.GetHeader("If-Modified-Since", DateTime.Now);

            /* Set content describing headers. */
            Response.Headers.Add("ETag", ETag);
            Response.Headers.Add("Last-Modified", ModifiedAt.ToString("r"));
            Response.Headers.Add("Content-Type", Type);

            /* Set cache-control header. */
            if (Request.Headers.ContainsKey("Authorization"))
                Response.Headers.Add("Cache-Control", "private, must-revalidate");
            else Response.Headers.Add("Cache-Control", "public, must-revalidate");

            /* test If-Match header. */
            if (IfMatch != null && IfMatch != ETag)
                return Controller.StatusCode(412);

            /* test If-None-Match header and If-Modified-Since header. */
            if ((IfNoneMatch != null && IfNoneMatch == ETag) ||
                (IfModifiedSince <= ModifiedAt))
                return Controller.StatusCode(304);

            /* finally transmit file contents. */
            return Controller.File(Content, Type);
        }

        /// <summary>
        /// Provide Built-In Assets with Cache-Control headers.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        private IActionResult Provide(ref byte[] Content, string Type)
            => Provide(this, Request, Response, RESP_ModifiedAt, RESP_ETag, ref Content, Type);

        /// <summary>
        /// Provide jQuery.js file.
        /// </summary>
        /// <returns></returns>
        [Route("assets/builtin/jquery.js")]
        public IActionResult jQuery() => Provide(ref RESP_jQuery, "text/js; charset=UTF-8");

        /// <summary>
        /// Provide Vue.js file.
        /// </summary>
        /// <returns></returns>
        [Route("assets/builtin/vue.js")]
        public IActionResult Vue() => Provide(ref RESP_Vue, "text/js; charset=UTF-8");
    }
}
