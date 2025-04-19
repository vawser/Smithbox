// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
using Andre.Core;

namespace Andre.Formats.Util
{
    static class ArchiveKeys
    {
        public static string GetKey(string bhdPath, Game game)
        {
            string name = Path.GetFileNameWithoutExtension(bhdPath);
            string dir = Path.GetDirectoryName(bhdPath) ?? "";
            if (game >= Game.ER && dir.Length >= 2 && Path.GetDirectoryName(bhdPath)![^2..] == "sd")
                name = "sd\\" + name;
            return game switch
            {
                Game.DS2 => File.ReadAllText(bhdPath.Replace("Ebl", "KeyCode").Replace(".bhd", ".pem")),
                Game.DS2S => File.ReadAllText(bhdPath.Replace("Ebl", "KeyCode").Replace(".bhd", ".pem")),
                Game.DS3 => DarkSouls3Keys[name],
                Game.SDT => SekiroKeys[name],
                Game.ER => EldenRingKeys[name],
                Game.AC6 => ArmoredCore6Keys[name],
                _ => throw new ArgumentException($"No keys for {game}")
            };
        }

        public static Dictionary<string, string> DarkSouls3Keys = new Dictionary<string, string>
        {
            ["Data1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEA05hqyboW/qZaJ3GBIABFVt1X1aa0/sKINklvpkTRC+5Ytbxvp18L
M1gN6gjTgSJiPUgdlaMbptVa66MzvilEk60aHyVVEhtFWy+HzUZ3xRQm6r/2qsK3
8wXndgEU5JIT2jrBXZcZfYDCkUkjsGVkYqjBNKfp+c5jlnNwbieUihWTSEO+DA8n
aaCCzZD3e7rKhDQyLCkpdsGmuqBvl02Ou7QeehbPPno78mOYs2XkP6NGqbFFGQwa
swyyyXlQ23N15ZaFGRRR0xYjrX4LSe6OJ8Mx/Zkec0o7L28CgwCTmcD2wO8TEATE
AUbbV+1Su9uq2+wQxgnsAp+xzhn9og9hmwIEC35bSQ==
-----END RSA PUBLIC KEY-----",

            ["Data2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvCZAK9UfPdk5JaTlG7n1r0LSVzIan3h0BSLaMXQHOwO7tTGpvtdX
m2ZLY9y8SVmOxWTQqRq14aVGLTKDyH87hPuKd47Y0E5K5erTqBbXW6AD4El1eir2
VJz/pwHt73FVziOlAnao1A5MsAylZ9B5QJyzHJQG+LxzMzmWScyeXlQLOKudfiIG
0qFw/xhRMLNAI+iypkzO5NKblYIySUV5Dx7649XdsZ5UIwJUhxONsKuGS+MbeTFB
mTMehtNj5EwPxGdT4CBPAWdeyPhpoHJHCbgrtnN9akwQmpwdBBxT/sTD16Adn9B+
TxuGDQQALed4S4KvM+fadx27pQz8pP9VLwIEL67iCQ==
-----END RSA PUBLIC KEY-----",

            ["Data3"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAqLytWD20TSXPeAA1RGDwPW18nJwe2rBX+0HPtdzFmQc/KmQlWrP+
94k6KClK5f7m0xUHwT8+yFGLxPdRvUPyOhBEnRA6tkObVDSxij5y0Jh4h4ilAO73
I8VMcmscS71UKkck4444+eR4vVd+SPlzIu8VgqLefvEn/sX/pAevDp7w+gD0NgvO
e9U6iWEXKwTOPB97X+Y2uB03gSSognmV8h2dtUFJ4Ryn5jrpWmsuUbdvGp0CWBKH
CFruNXnfsG0hlf9LqbVmEzbFl/MhjBmbVjjtelorZsoLPK+OiPTHW5EcwwnPh1vH
FFGM7qRMc0yvHqJnniEWDsSz8Bvg+GxpgQIEC8XNVw==
-----END RSA PUBLIC KEY-----",

            ["Data4"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEArfUaZWjYAUaZ0q+5znpX55GeyepawCZ5NnsMjIW9CA3vrOgUGRkh
6aAU9frlafQ81LQMRgAznOnQGE7K3ChfySDpq6b47SKm4bWPqd7Ulh2DTxIgi6QP
qm4UUJL2dkLaCnuoya/pGMOOvhT1LD/0CKo/iKwfBcYf/OAnwSnxMRC3SNRugyvF
ylCet9DEdL5L8uBEa4sV4U288ZxZSZLg2tB10xy5SHAsm1VNP4Eqw5iJbqHEDKZW
n2LJP5t5wpEJvV2ACiA4U5fyjQLDzRwtCKzeK7yFkKiZI95JJhU/3DnVvssjIxku
gYZkS9D3k9m+tkNe0VVrd4mBEmqVxg+V9wIEL6Y6tw==
-----END RSA PUBLIC KEY-----",

            ["Data5"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvKTlU3nka4nQesRnYg1NWovCCTLhEBAnjmXwI69lFYfc4lvZsTrQ
E0Y25PtoP0ZddA3nzflJNz1rBwAkqfBRGTeeTCAyoNp/iel3EAkid/pKOt3JEkHx
rojRuWYSQ0EQawcBbzCfdLEjizmREepRKHIUSDWgu0HTmwSFHHeCFbpBA1h99L2X
izH5XFTOu0UIcUmBLsK6DYsIj5QGrWaxwwXcTJN/X+/syJ/TbQK9W/TCGaGiirGM
1u2wvZXSZ7uVM3CHwgNhAMiqLvqORygcDeNqxgq+dXDTxka43j7iPJWdHs8b25fy
aH3kbUxKlDGaEENNNyZQcQrgz8Q76jIE0QIEFUsz9w==
-----END RSA PUBLIC KEY-----",

            ["DLC1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAsCGM9dFwzaIOUIin3DXy7xrmI2otKGLZJQyKi5X3znKhSTywpcFc
KoW6hgjeh4fJW24jhzwBosG6eAzDINm+K02pHCG8qZ/D/hIbu+ui0ENDKqrVyFhn
QtX5/QJkVQtj8M4a0FIfdtE3wkxaKtP6IXWIy4DesSdGWONVWLfi2eq62A5ts5MF
qMoSV3XjTYuCgXqZQ6eOE+NIBQRqpZxLNFSzbJwWXpAg2kBMkpy5+ywOByjmWzUw
jnIFl1T17R8DpTU/93ojx+/q1p+b1o5is5KcoP7QwjOqzjHJH8bTytzRbgmRcDMW
3ahxgI070d45TMXK2YwRzI6/JbM1P29anQIEFezyYw==
-----END RSA PUBLIC KEY-----",

            ["DLC2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAtCXU9a/GBMVoqtpQox9p0/5sWPaIvDp8avLFnIBhN7vkgTwulZHi
u64vZAiUAdVeFX4F+Qtk+5ivK488Mu2CzAMJcz5RvyMQJtOQXuDDqzIv21Tr5zuu
sswoErHxxP8TZNxkHm7Ram7Oqtn7LQnMTYxsBgZZ34yJkRtAmZnGoCu5YaUR5euk
8lF75idi97ssczUNV212tLzIMa1YOV7sxOb7+gc0VTIqs3pa+OXLPI/bMfwUc/KN
jur5aLDDntQHGx5zuNtc78gMGwlmPqDhgTusKPO4VyKvoL0kITYvukoXJATaa1HI
WVUjhLm+/uj8r8PNgolerDeS+8FM5Bpe9QIEHwCZLw==
-----END RSA PUBLIC KEY-----",
        };

        public static Dictionary<string, string> SekiroKeys = new Dictionary<string, string>
        {
            ["Data1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEA92l+AWx1aV7mzt+6r00bm/qnc4b6NH3VVr/v4UxMcfzushL8jsn9
ZSP1ss95ot/quk8dOJsp0+/bvxH+C9DEezzNLSqqAGd2jq2PYosj/6FhYAKjjMlK
jNxcVPsKQug0Zby+KYsENirmEXcmA1fzltrISf6d6LKB1UFHHN9NRkLCm3idE4Pu
9852kPHbiL14EqfDCDgwm7kLeQdt3kUbcmdhu/6dvP42HGxBmAYLNFD3iAe7qLML
MFzmKKHQD2fRQK/431Z3xPK6Jp245AdR0AwUYVvnXq+/97wMX0C6UKvAZ+b/1ytD
Nu8vZt++lhJ01SjTc2A4hVPz7g1EEO5/TQIEKkj5Jw==
-----END RSA PUBLIC KEY-----",

            ["Data2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEAqhjoThWX8VwsTKTI1kjp0JBloCXhV8i99P1KPTCTDBnmhVQPdu+7
UQ5g4//eh0oqKaOUjet+0SP94QscjIIrhV91OzfIouIWgJJK/ROOP/A3sb5AlzPa
6YPcN8ODxR+esyrWhc6rHCt4qGvXVXrgh6zpZM5h5VCTSaup4qqIWm44EF3+FeYS
7faFg14rH0QEosieIIZFZmpI6SCJanlrVd+Zh13s4XcZfk0JdC2AEjxCQ2lKi3Un
WAMOcJc+8uHoMuNNo1PMpYQ6Z8Nzg5Cii7EnwbCDmuJw58tFBmbOVHZpkY93VIeF
maJXSE7ztTp0qTa05YZUsiU3g9HplkeTUwIFAP/xKZE=
-----END RSA PUBLIC KEY-----",

            ["Data3"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEAx5jlgIvoHQLwSFsAwKFZbNo3fgZ89C7tj4hwiZsQVg8QnNZohXl5
S5Ep9pS2biOFsSkuZMXKmfYErh2CsdFbr7QR7kvPPianXNrkCI4xlfQwJvMmkLm9
6/JmRIUzTWp0kKJUJZJH/UIrXNn7fmk8Vmx1bQIi8bumGSl3gxeMhutv/lC9khsY
Tn0ABTJAbIbwNZ5GPXxzQZuQPXXDY52Gm+Fx7Yy1LiK/B6isIDJUN0xdgxdaXxGN
f5pPocMJjng0Ob3cjhGvdkysll/jYFnRx0La3CGmtLcXMtHheEQxzGueGDa/lkkl
AvvEXtcpKfyFQWcUheQZ8LngAh/UTJHtQwIFAOpVoU8=
-----END RSA PUBLIC KEY-----",

            ["Data4"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAq8RyArk+eqMAcxLAHUDRYV7yScNKZpKSxGmgJZQ7y6Y8f5wdrNCt
byXfmsdQECStIGlkwWjtfm8t/bRZuxxPciAYaFsWo0Ze2BB6uY6ZteNpLJn82qbL
TXATf+af3kSrvICfvJwRzbfA/PRJRkHj2gJ6Tc7g6HK7S/4TiCZirq+c/zLY3gb8
A8uIFNI4j0qxTzfoAlS7K6spZjfnhZ6l7pYFh+glz15wAbppC9Oy/u5vUacozf4v
nacbUHD47ds9EZPZDHk3LfJbioHwtUzJfyBqZmIpI33yiwImPpb96zwvQU86TaXK
sJrTmSs/48BeDsQwXuaqOg+6noETBx3pgQIEGM2Ohw==
-----END RSA PUBLIC KEY-----",

            ["Data5"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEAu75/UbXwHdvu/p49TwnY7Ou6DAuZYFAtLUkw/R4nvm0HWVlRsZiB
LG3MOG6sPmK2Zc3JLBU2QK4uKazZ9VrmotM4OpYr03q2tiFnv3NfCvB1UeIJIKe3
kVhHNZIbvrwEP9a5UCnrSHD+u+Fj5MQBr4yrEitwrNVvIC4J0Ez1Ppn3+D8ff8Xg
QRP9qCVLI3X/wdQDea+B5o8PWaYEL9MKnnL1Tq4h+4PRYHcQR8/GXBTrc3x9q3cP
QRDWHbRYhIfWSP9urtagjcsmcuG+p34fp+KyWOwkil3FJqwH1KgSTbk9Tb0oBPzq
TCJKeE/wgu6hY++lBi5T3ArHZZcsbXzV6wIFAPlRTMc=
-----END RSA PUBLIC KEY-----",
        };

        public static Dictionary<string, string> SekiroBonusKeys = new Dictionary<string, string>
        {
            ["Data"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEAxFOPK7c3E2Tu8HSS3NUlWUHdlJZIJiHf/0DhyLZUP68iEhJ8SLQ0
sDAgFBxIAEZQcVZBKLhTZiTyqaqIolgCDH6ZcMlWOGOWj3G6PuhPeb/7ZeQSo7Xv
plGCovqnioRoaFf4gVZDsbpVXIGNXwWsL5kArQiQo3ZrMs17/t77yZ6avC/1hnFp
ks1k3uQ269NKZOpU6Q73I8yolUFGJFBlm9uHqRfZC0wcA+IXjo96C1PoTKJQktkh
J07MPeoeckkAGdUv9S+kcDN04SAGMJJBWB9OOvn2Qle938gmCY6beeuk8c/l67zs
ChgwGmsLdVr7W6hZL3aNvsf/BWFQ+e7+tQIFANZbM50=
-----END RSA PUBLIC KEY-----",
        };

        public static Dictionary<string, string> EldenRingKeys = new Dictionary<string, string>
        {
            ["Data0"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEA9Rju2whruXDVQZpfylVEPeNxm7XgMHcDyaaRUIpXQE0qEo+6Y36L
P0xpFvL0H0kKxHwpuISsdgrnMHJ/yj4S61MWzhO8y4BQbw/zJehhDSRCecFJmFBz
3I2JC5FCjoK+82xd9xM5XXdfsdBzRiSghuIHL4qk2WZ/0f/nK5VygeWXn/oLeYBL
jX1S8wSSASza64JXjt0bP/i6mpV2SLZqKRxo7x2bIQrR1yHNekSF2jBhZIgcbtMB
xjCywn+7p954wjcfjxB5VWaZ4hGbKhi1bhYPccht4XnGhcUTWO3NmJWslwccjQ4k
sutLq3uRjLMM0IeTkQO6Pv8/R7UNFtdCWwIERzH8IQ==
-----END RSA PUBLIC KEY-----",

            ["Data1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAxaBCHQJrtLJiJNdG9nq3deA9sY4YCZ4dbTOHO+v+YgWRMcE6iK6o
ZIJq+nBMUNBbGPmbRrEjkkH9M7LAypAFOPKC6wMHzqIMBsUMuYffulBuOqtEBD11
CAwfx37rjwJ+/1tnEqtJjYkrK9yyrIN6Y+jy4ftymQtjk83+L89pvMMmkNeZaPON
4O9q5M9PnFoKvK8eY45ZV/Jyk+Pe+xc6+e4h4cx8ML5U2kMM3VDAJush4z/05hS3
/bC4B6K9+7dPwgqZgKx1J7DBtLdHSAgwRPpijPeOjKcAa2BDaNp9Cfon70oC+ZCB
+HkQ7FjJcF7KaHsH5oHvuI7EZAl2XTsLEQIENa/2JQ==
-----END RSA PUBLIC KEY-----",

            ["Data2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEA0iDVVQ230RgrkIHJNDgxE7I/2AaH6Li1Eu9mtpfrrfhfoK2e7y4O
WU+lj7AGI4GIgkWpPw8JHaV970Cr6+sTG4Tr5eMQPxrCIH7BJAPCloypxcs2BNfT
GXzm6veUfrGzLIDp7wy24lIA8r9ZwUvpKlN28kxBDGeCbGCkYeSVNuF+R9rN4OAM
RYh0r1Q950xc2qSNloNsjpDoSKoYN0T7u5rnMn/4mtclnWPVRWU940zr1rymv4Jc
3umNf6cT1XqrS1gSaK1JWZfsSeD6Dwk3uvquvfY6YlGRygIlVEMAvKrDRMHylsLt
qqhYkZNXMdy0NXopf1rEHKy9poaHEmJldwIFAP////8=
-----END RSA PUBLIC KEY-----",

            ["Data3"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvRRNBnVq3WknCNHrJRelcEA2v/OzKlQkxZw1yKll0Y2Kn6G9ts94
SfgZYbdFCnIXy5NEuyHRKrxXz5vurjhrcuoYAI2ZUhXPXZJdgHywac/i3S/IY0V/
eDbqepyJWHpP6I565ySqlol1p/BScVjbEsVyvZGtWIXLPDbx4EYFKA5B52uK6Gdz
4qcyVFtVEhNoMvg+EoWnyLD7EUzuB2Khl46CuNictyWrLlIHgpKJr1QD8a0ld0PD
PHDZn03q6QDvZd23UW2d9J+/HeBt52j08+qoBXPwhndZsmPMWngQDaik6FM7EVRQ
etKPi6h5uprVmMAS5wR/jQIVTMpTj/zJdwIEXszeQw==
-----END RSA PUBLIC KEY-----",

            ["DLC"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAmYJ/5GJU4boJSvZ81BFOHYTGdBWPHnWYly3yWo01BYjGRnz8NTkz
DHUxsbjIgtG5XqsQfZstZILQ97hgSI5AaAoCGrT8sn0PeXg2i0mKwL21gRjRUdvP
Dp1Y+7hgrGwuTkjycqqsQ/qILm4NvJHvGRd7xLOJ9rs2zwYhceRVrq9XU2AXbdY4
pdCQ3+HuoaFiJ0dW0ly5qdEXjbSv2QEYe36nWCtsd6hEY9LjbBX8D1fK3D2c6C0g
NdHJGH2iEONUN6DMK9t0v2JBnwCOZQ7W+Gt7SpNNrkx8xKEM8gH9na10g9ne11Mi
O1FnLm8i4zOxVdPHQBKICkKcGS1o3C2dfwIEXw/f3w==
-----END RSA PUBLIC KEY-----",

            ["sd\\sd"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAmYJ/5GJU4boJSvZ81BFOHYTGdBWPHnWYly3yWo01BYjGRnz8NTkz
DHUxsbjIgtG5XqsQfZstZILQ97hgSI5AaAoCGrT8sn0PeXg2i0mKwL21gRjRUdvP
Dp1Y+7hgrGwuTkjycqqsQ/qILm4NvJHvGRd7xLOJ9rs2zwYhceRVrq9XU2AXbdY4
pdCQ3+HuoaFiJ0dW0ly5qdEXjbSv2QEYe36nWCtsd6hEY9LjbBX8D1fK3D2c6C0g
NdHJGH2iEONUN6DMK9t0v2JBnwCOZQ7W+Gt7SpNNrkx8xKEM8gH9na10g9ne11Mi
O1FnLm8i4zOxVdPHQBKICkKcGS1o3C2dfwIEXw/f3w==
-----END RSA PUBLIC KEY-----",

            ["sd\\sd_dlc02"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAmYJ/5GJU4boJSvZ81BFOHYTGdBWPHnWYly3yWo01BYjGRnz8NTkz
DHUxsbjIgtG5XqsQfZstZILQ97hgSI5AaAoCGrT8sn0PeXg2i0mKwL21gRjRUdvP
Dp1Y+7hgrGwuTkjycqqsQ/qILm4NvJHvGRd7xLOJ9rs2zwYhceRVrq9XU2AXbdY4
pdCQ3+HuoaFiJ0dW0ly5qdEXjbSv2QEYe36nWCtsd6hEY9LjbBX8D1fK3D2c6C0g
NdHJGH2iEONUN6DMK9t0v2JBnwCOZQ7W+Gt7SpNNrkx8xKEM8gH9na10g9ne11Mi
O1FnLm8i4zOxVdPHQBKICkKcGS1o3C2dfwIEXw/f3w==
-----END RSA PUBLIC KEY-----",

        };

        public static Dictionary<string, string> ArmoredCore6Keys = new Dictionary<string, string>
        {
            ["Data0"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEA7F43Ss9kroawBSUW6GBhSUo6GtxYtUV8zCPkcHhSJLGPASHhwsaX
zMRrd+Ul9qB3oYchb4xYtdMWKFe0/ZDi9vgYXvF3rlWaZKAu1k/F6RwVAd//I3Kj
JsYlhayskInKqB3BvB/KL2Ga8QBsZ/G9cLlUYsqIj3as9oqbfEXVmGVeuhg0I+NQ
NL+2sThqp5eOQstfXQgqduOt0ixd/r9e5VjLhyj2z4hCEF2TVsDw9wGEBem1TkcO
C/E8obl9fTHwEK7l2i8a4HafY7flU220r8y4UwQ+9Aq94xUYT2xdcTjdyBIaZtyS
YmR86B680OyL9oiEonEFhh4cor/84PSmNQIFAOHX27k=
-----END RSA PUBLIC KEY-----",

            ["Data1"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAsM5QTi6Fo1li23fkvP7jXqFlanR61VS1znElmsZH0Ez4LtuM7WUC
QnZyi9u15T89WmIKCGpfgHZBgJEVFqW9FMhBxrA5/gXcqnGESjc+NNF71rfug/qg
Ue7B9tXlq18/bdD7qPEjYY02H5fh4Z/g0+oClSNyZR46G/MXZSw8KMV4QHikCAxJ
N40Nxd+MpQcpc3J5SXfsXxi9gNSxHO1+KzGwRrEh1/9d7bPyd4jBuTR+SEd+ZDHR
3jTbbRNUypB/x780KXuJnGrC8UfB6ttxfBmLs7nmhteO6R1rr5zWuHJBry7Of9t4
JEQRDwb2VT3fpQ2oHgOc5zDYMOdObdX/tQIEZYdnGQ==
-----END RSA PUBLIC KEY-----",

            ["Data2"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEApvNV8cCaxTBtW6kB94Gd5/+NuQnVLxRo6b0QUSSXh8KGWCRNPjpq
LEyu0kuHCTG5xfomzB5vlq9INu0odZAWZu+NWvz+YydnIQO+UPDF9J/wE92SMzBj
n7d4uEglevPswQLiJVQThCtrl1B8dCz7vFvlSknx23jdUQ/0hfxVnLvP2GpNW/v5
iDK+J2RJFxpd8td9FpHMF+OMxT3pvQyOBleWgEcmiaA1O6AxZA3YGWaL7qnKgx4M
Pi5Ex1Cjnw66+A25kc34UvDA4pteJHC+AwTvjLN3nF2jr3l61jEcXULCWpA4rdWT
dm77dL2KwxXDiYxNAecEFVuG/PRV7J8hHQIFAP3yuSk=
-----END RSA PUBLIC KEY-----",

            ["Data3"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBDAKCAQEAvS4XXheufoQvvfTksJhO7JrH1ykMa2ogHtqHhAOZXorLLXvsWfVO
Q+enFY7kBjKyVbUOlqj3M6Ho2S8QgJTO/Xz3DhD3YlHva59RnIXI2gmSyvrTB0Z/
GopmJzVfc9o7763CMy/27tS4/dBM9qKs+csvjE6fG470Z025yECgtTtXzltg4pht
GkjV5+tNjrFt5+wxIydNB56Xow9QCxtpZJ4TstdZvbgq1K06mpLrRTRDxpLPgdDA
9KKwyewYliU2tl78bU5jcgL3s78KbiJ2VSrlOL2AxI6TpID+kFcDy055JsMkKR2V
nRPreV08oQchzQ5miTezWUAk7mIcZoFHwwIFAP////8=
-----END RSA PUBLIC KEY-----",

            ["sd\\sd"] =
@"-----BEGIN RSA PUBLIC KEY-----
MIIBCwKCAQEAvbz2HnG3JaP3imwnZJMWCyzW9ZjrgzgxZ/EtzFrzpeYPFdYGmBim
XFfBarWpREW7M4y6Z7iy4UAbNmF9SLDp6RLiKauI9AxQK3ICYf+2UuDaXO6QeeXM
JgwQhIugkQFobZOnJLpGT4ynWvH0iR2LO/aFivAPry+bkKXCX9y5zfod2at6j0Ri
7jJ+SECnWs7rDxsBo/98aJpmoyl+Z24yvBU+sknUVn7giaFPTOet9YvBzNmAing4
6fBFXV6T4sX+hyCY2Qs2arnH+TSDTUcxK/3lobC7lSy+B+sTinqN0a+SWeaCACYe
viCKV6AQNxb8J1CHev0OJv9r+bMSy+1kxwIEILt78w==
-----END RSA PUBLIC KEY-----",


        };

    }
}