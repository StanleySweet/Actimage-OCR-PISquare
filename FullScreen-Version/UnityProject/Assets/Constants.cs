namespace Assets
{
    using System;
    using UnityEngine;
    class Constants
    {
        public static Color PERFECT_MATCH_COLOR = Color.green;
        public static Color APPROXIMATE_MATCH_COLOR = Color.yellow;
        public static String VALIDATION_TEXT = "okay";
        public static String STOP_TEXT = "stop";
        public static String WILDCARD = "quarante deux";
        public static String SEARCH_KEYWORD = "rechercher";
        public static String QRCODE_KEYWORD = "traduire";
        public static String SEARCHING_FOR_QR_CODES = "Recherche de QR codes en cours";
        public static String HOME_PROMPT = string.Format("Donnez le mot à rechercher. \n Dites '{0}' pour annuler.", STOP_TEXT);
        public static String LOOKING_FOR_WORD = "Recherche en cours pour le mot ";
        public static String RESET_IN_PROGRESS = "Réinitialisation en cours";
        public static String NO_QR_CODE_FOUND = "Aucun QR code trouvé, " + RESET_IN_PROGRESS;
        public static Double TIMEOUT = 5;
        public static Boolean DEBUG = true;
        public static String ERROR_NO_RESULT_FOUND = "ERROR : 0 exact match(es) were found. 0 close match(es) were found for the word";
    }
}
