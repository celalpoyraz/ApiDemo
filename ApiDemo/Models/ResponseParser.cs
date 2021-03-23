﻿using ApiDemo.Model;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serialization.Json;

namespace ApiDemo
{
    public static class ResponseParser
    {
        public static GeneralResponse<T> Parse<T>(this IRestResponse response)
        {
            JsonDeserializer restcsharpDeserializer = new JsonDeserializer();

            var result = new GeneralResponse<T>();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                if (typeof(T) == typeof(byte[]))
                {
                    object bytes = response.RawBytes;
                    return new GeneralResponse<T>()
                    {
                        Result = (T)bytes
                    };
                }
                else
                {
                    return restcsharpDeserializer.Deserialize<GeneralResponse<T>>(response);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(response.Content))
                {
                    result = new GeneralResponse<T>() { Result = result.Result };
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        result.ErrorStatus = new Status() { Code = (int)response.StatusCode, Message = "API Istek Yolu Hatalı" };
                    }
                    else
                    {
                        result.ErrorStatus = restcsharpDeserializer.Deserialize<Status>(response);
                        result.ErrorStatus.Code = (int)response.StatusCode;
                    }
                    return result;
                }
                else//sunucunun cevap vermediği durumlar
                {
                    return new GeneralResponse<T>()
                    {
                        Result = result.Result,
                        ErrorStatus = new Status()
                        {
                            Code = (int)response.StatusCode,
                            Message = response.ErrorException != null ? response.ErrorException.Message : null
                        }
                    };
                }
            }
        }
    }
}