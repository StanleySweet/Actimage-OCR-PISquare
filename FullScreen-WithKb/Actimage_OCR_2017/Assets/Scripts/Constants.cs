﻿namespace Assets
{
    using System;
    using UnityEngine;
    static class Constants
    {
        #region Critical Constants
        public static readonly String WILDCARD_HEARD_TEXT = "42";
        public static readonly String WILDCARD_TEXT = "quarante deux";
        public static readonly String STOP_KEYWORD = "stop";
        public static readonly Double TIMEOUT = 10;
        public static readonly Boolean DEBUG = false;
        public static readonly Boolean ENGLISH_PROMPTS = false;
        public static readonly String SEARCH_KEYWORD = ENGLISH_PROMPTS ? "search for" : "rechercher";
        public static readonly String QRCODE_KEYWORD = ENGLISH_PROMPTS ? "translate" : "traduire";
        public static readonly String VALIDATION_KEYWORD = ENGLISH_PROMPTS ? "OK" : "okay";
        public static readonly String CORRECTION_KEYWORD = ENGLISH_PROMPTS ? "change" : "changer";
        #endregion

        #region 
        public static readonly String TEXT_INPUT_MESH_LABEL = "EmailText";
        public static readonly String TEXT_PROMPT_MESH_LABEL = "EmailTitle";
        public static readonly String WEBCAM_HAS_BEEN_INITIALIZED_SUCESSFULLY = "La webcam a été initialisée correctement.";
        public static readonly String WEBCAM_HAS_NOT_BEEN_INITIALIZED_SUCESSFULLY = "La webcam n'a pas été initialisée correctement.";
        public static readonly Color32 PERFECT_MATCH_COLOR = Color.green;
        public static readonly Color32 APPROXIMATE_MATCH_COLOR = Color.yellow;
        public static readonly float DEBUG_PLANE_WIDTH = 0.04700003F;
        public static readonly float DEBUG_PLANE_HEIGHT = 0.03750001F;
        public static readonly String DEFAULT_VR_DEVICE_NAME = string.Empty;
        public static readonly String DEFAULT_MICROPHONE_DEVICE_NAME = string.Empty;
        public static readonly String IS_MICROPHONE_RECORDING = "Le Microphone " + (Microphone.IsRecording(DEFAULT_MICROPHONE_DEVICE_NAME) ? "est" : "n'est pas ") + " en train d'enregistrer.";
        public static readonly String EXACT_MATCHES_WERE_FOUND = " occurence(s) exacte(s) ont été trouvée(s). \n ";
        public static readonly String APPROXIMATE_MATCHES_WERE_FOUND = " occurence(s) proche(s) ont été trouvée(s). \n pour le mot : ";
        public static readonly String RESULT_TEXT_RECOGNITION_SENTENCE = "Recherche pendant " + TIMEOUT + " secondes . \n {0}" + EXACT_MATCHES_WERE_FOUND + "{1}" + APPROXIMATE_MATCHES_WERE_FOUND + "' {2} '.";
        public static readonly String THE_WORD_YOU_ARE_LOOKING_FOR_IS = "Le mot à rechercher est {0} \n {1}";
        public static readonly String PLANE_MESH_NAME = "Plane";
        public static readonly String CANVAS_MESH_NAME = "Canvas";
        public static readonly String SEARCH_IS_OVER = "\n Fin de la recherche dites '" + STOP_KEYWORD + "' pour retourner au menu.";
        public static readonly String THE_QR_CODE_MEANS = "Le QR code représente : {0}\n Dites '" + STOP_KEYWORD + "' pour retourner au menu";
        public static readonly String RESULT_BOX_NAME_PREFIX = "ResultBox";
        public static readonly String DEFAULT_MATERIAL_LOCATION = "/HoloToolkit/Input/Models/Cursor/Materials/lambert1.mat";
        public static readonly String TEXT_SHADER_LOCATION = "UI/Default Font";
        public static readonly String TEXT_QUITTING_APPLICATION = "Arrêt de l'application";
        public static readonly String FINAL_WORD_SEARCH_PROMPT = String.Format("dites '{1}' pour continuer. \n Dites '{0}' pour annuler.\n Dites '{2}' pour corriger", STOP_KEYWORD, VALIDATION_KEYWORD, CORRECTION_KEYWORD);
        public static readonly String SEARCHING_FOR_QR_CODES = "Recherche de QR codes en cours";
        public static readonly String STOP_PROMPT = string.Format("Dites '{0}' pour annuler.", STOP_KEYWORD);
        public static readonly String SEARCH_PROMPT = string.Format("Donnez le mot à rechercher. \n Dites '{0}' pour annuler.", STOP_KEYWORD);
        public static readonly String LOOKING_FOR_WORD = "Recherche en cours pour le mot '{0}'.";
        public static readonly String RESET_IN_PROGRESS = "Réinitialisation en cours";
        public static readonly String NO_QR_CODE_FOUND = "Aucun QR code trouvé, \n Dites '" + STOP_KEYWORD + "' pour retourner au menu ";
        public static readonly String HOME_PROMPT = string.Format("Tapez dans la barre le mot à rechercher et cliquez sur le bouton scan, ou \n Dites '{0}' pour commencer \n ou '{1}' pour dechiffrer un QR code", SEARCH_KEYWORD, QRCODE_KEYWORD);
        public static readonly String ERROR_DISPLAYING_RECTANGLES = "ERREUR : Erreur lors de l'affichage du/des rectangle(s). ";
        public static readonly String ERROR_LOOKING_FOR_WORDS = "ERREUR : Erreur lors de la recherche de mots";
        public static readonly String ERROR_COMPLETING_DICTATION = "ERREUR : La dictée ne s'est pas terminée correctement: {0} .";
        public static readonly String ERROR_NO_INTERNET = "ERREUR : La reconnaissance vocale ne fonctionne pas sans internet.";
        public static readonly String ERROR_IN_DICTATION = "ERREUR : La dictée a échoué : {0} ; HResult = {1} " + ".";
        public static readonly String ERROR_NO_RESULT_FOUND = "ERREUR : aucune occurence exacte ni proche n'ont été trouvées.";
        #endregion
    }
}