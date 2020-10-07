import java.net.URLEncoder;
import java.nio.charset.StandardCharsets;
import java.time.Instant;
import java.util.Base64;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;

// https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-security#security-tokens
class MakeSas {
    public static String resourceUri = "poorlyfundedskynet.azure-devices.net/devices/";

    public static String getSASToken(String resourceUri, String key) throws Exception {
        String stringToSign, token;
        var expiry = Instant.now().getEpochSecond() + 3600;

            stringToSign = URLEncoder.encode(resourceUri, StandardCharsets.UTF_8) + "\n" + expiry;
            byte[] decodedKey = Base64.getDecoder().decode(key);

            Mac sha256HMAC = Mac.getInstance("HmacSHA256");
            SecretKeySpec secretKey = new SecretKeySpec(decodedKey, "HmacSHA256");
            sha256HMAC.init(secretKey);
            Base64.Encoder encoder = Base64.getEncoder();

            String signature = new String(encoder.encode(
                sha256HMAC.doFinal(stringToSign.getBytes(StandardCharsets.UTF_8))), StandardCharsets.UTF_8);

            token = "SharedAccessSignature sr=" + URLEncoder.encode(resourceUri, StandardCharsets.UTF_8)
                    + "&sig=" + URLEncoder.encode(signature, StandardCharsets.UTF_8.name()) + "&se=" + expiry;

            return token;
    }


    public static void main(String[] arguments) {
        var token = "";
        try {
            token = getSASToken("poorlyfundedskynet.azure-devices.net/devices/yolo", "DevicePrimaryKeyGoesHere=");
            System.out.println(token);
        }
        catch (Exception e) {
            System.out.println(e);
        }
    }
}
