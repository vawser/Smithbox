// The part of NavSurvey's SurveyGame that Overlay.cs uses, without the navigation profile the pre-N1 sources lack: ER only
public static class SurveyGame
{
    public static string Key => "ER";

    public static Havok.HavokProfile Profile => Havok.HavokProfile.FromGame(Key);
}
