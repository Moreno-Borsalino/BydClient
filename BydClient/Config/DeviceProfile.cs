using System;
using System.Collections.Generic;
using System.Text;
using BydClient.Crypto;

namespace BydClient.Config;

/// <summary>
/// Device identity fields sent with every request.
/// </summary>
public class DeviceProfile(
    string OsType = "15",
    string Imei = "BANGCLE01234",
    string Mac = "00:00:00:00:00:00",
    string Model = "POCO F1",
    string Sdk = "35",
    string Mod = "Xiaomi",
    string ImeiMd5 = "00000000000000000000000000000000",
    string MobileBrand = "XIAOMI",
    string MobileModel = "POCO F1",
    string DeviceType = "0",
    string NetworkType = "wifi",
    string OsVersion = "35"
)
{
    public string OsType { get; init; } = OsType;
    public string Imei { get; init; } = Imei;
    public string Mac { get; init; } = Mac;
    public string Model { get; init; } = Model;
    public string Sdk { get; init; } = Sdk;
    public string Mod { get; init; } = Mod;
    public string ImeiMd5 { get; set; } = ImeiMd5;
    public string MobileBrand { get; init; } = MobileBrand;
    public string MobileModel { get; init; } = MobileModel;
    public string DeviceType { get; init; } = DeviceType;
    public string NetworkType { get; init; } = NetworkType;
    public string OsVersion { get; init; } = OsVersion;

    public void Deconstruct(out string OsType, out string Imei, out string Mac, out string Model, out string Sdk, out string Mod, out string ImeiMd5, out string MobileBrand, out string MobileModel, out string DeviceType, out string NetworkType, out string OsVersion)
    {
        OsType = this.OsType;
        Imei = this.Imei;
        Mac = this.Mac;
        Model = this.Model;
        Sdk = this.Sdk;
        Mod = this.Mod;
        ImeiMd5 = this.ImeiMd5;
        MobileBrand = this.MobileBrand;
        MobileModel = this.MobileModel;
        DeviceType = this.DeviceType;
        NetworkType = this.NetworkType;
        OsVersion = this.OsVersion;
    }

    public DeviceProfile(string username) : this()
    {
        if(ImeiMd5 == "00000000000000000000000000000000")
            ImeiMd5 = Hashing.Md5Hex(username);
    }

    public void SetImeiMd5FromUsername(string username)
    {
        if(ImeiMd5 == "00000000000000000000000000000000")
            ImeiMd5 = Hashing.Md5Hex(username);
    }
}

//public void SetImeiMd5FromUsername(string username)
//{
//    ImeiMd5 = Hashing.Md5Hex(username).ToUpperInvariant();
//}
// Please note ! The login failure (code 1033 "Network busy") is caused by a hardcoded imeiMD5 of all zeros.
// the imeiMD5 field must be derived from MD5(username).toUpperCase()
// https://github.com/jkaberg/pyBYD/issues/30