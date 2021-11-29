using Dapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace WinkNatural.Web.Services.Utilities
{
    public static class ProductImageUtility
    {
        public static FileContentResult GetProductImageUtility(string imageName)
        {
            try
            {
                object bytes;
                using (var context = Common.Utils.DbConnection.Sql())
                {
                    var query = @"SELECT TOP 1 
                                    ImageData 
                                  FROM 
                                    ItemImages 
                                  WHERE 
                                    ImageName = @Name";

                    bytes = context.ExecuteScalar(query, new { Name = imageName });
                }
                var extension = Path.GetExtension(imageName).ToLower();
                string contentType = "image/jpeg";

                switch (extension)
                {
                    case ".gif":
                        contentType = "image/gif";
                        break;
                    case ".jpg":
                        contentType = "image/jpeg";
                        break;
                    case ".jpeg":
                        contentType = "image/png";
                        break;
                    case ".bmp":
                        contentType = "image/bmp";
                        break;
                    case ".png":
                        contentType = "image/png";
                        break;
                }

                return new FileContentResult((byte[])bytes, contentType);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }
    }
}
