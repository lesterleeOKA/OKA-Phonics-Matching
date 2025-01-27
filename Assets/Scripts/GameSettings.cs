using SimpleJSON;
using System;

[Serializable]
public class GameSettings : Settings
{
    public int playerNumber = 0;
    public int timeToViewCards = 0;
    public string card_image_front;
    public string card_image_bottom;
    public int pairOfEachPage;
}

public static class SetParams
{
    public static void setCustomParameters(GameSettings settings = null, JSONNode jsonNode= null)
    {
        if (settings != null && jsonNode != null)
        {
            ////////Game Customization params/////////
            string card_image_front = jsonNode["setting"]["card_image_front"] != null ?
                jsonNode["setting"]["card_image_front"].ToString().Replace("\"", "") : null;

            string card_image_bottom = jsonNode["setting"]["card_image_bottom"] != null ?
                jsonNode["setting"]["card_image_bottom"].ToString().Replace("\"", "") : null;

            settings.timeToViewCards = jsonNode["setting"]["time_To_View_Cards"] != null ? jsonNode["setting"]["time_To_View_Cards"] : null;
            LoaderConfig.Instance.gameSetup.timeToViewCards = settings.timeToViewCards;

            settings.pairOfEachPage = jsonNode["setting"]["pair_number"] != null ? jsonNode["setting"]["pair_number"] : 5;
            LoaderConfig.Instance.gameSetup.pairOfEachPage = settings.pairOfEachPage;

            if (card_image_front != null)
            {
                if (!card_image_front.StartsWith("https://") || !card_image_front.StartsWith(APIConstant.blobServerRelativePath))
                    settings.card_image_front = APIConstant.blobServerRelativePath + card_image_front;
            }

            if (card_image_bottom != null)
            {
                if (!card_image_bottom.StartsWith("https://") || !card_image_bottom.StartsWith(APIConstant.blobServerRelativePath))
                    settings.card_image_bottom = APIConstant.blobServerRelativePath + card_image_bottom;
            }
        }
    }
}

