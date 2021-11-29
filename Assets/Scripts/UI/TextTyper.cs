using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Type text component types out Text one character at a time. Heavily adapted from synchrok's GitHub project.
/// </summary>
[RequireComponent(typeof(Text))]
public sealed class TextTyper : MonoBehaviour
{
    /// <summary>
    /// The print delay setting. Could make this an option some day, for fast readers.
    /// </summary>
    private const float PrintDelaySetting = 0.02f;

    // Characters that are considered punctuation in this language. TextTyper pauses on these characters
    // a bit longer by default. Could be a setting sometime since this doesn't localize.
    private readonly List<char> punctutationCharacters = new List<char> { '.', ',', '!', '?' };

    [SerializeField]
    [Tooltip("Event that's called when the text has finished printing.")]
    private UnityEvent printCompleted = new UnityEvent();

    [SerializeField]
    [Tooltip("Event called when a character is printed. Inteded for audio callbacks.")]
    private CharacterPrintedEvent characterPrinted = new CharacterPrintedEvent();

    private Text textComponent;
    private string printingText;
    private float defaultPrintDelay;
    private Coroutine typeTextCoroutine;

    /// <summary>
    /// Gets the PrintCompleted callback event.
    /// </summary>
    /// <value>The print completed callback event.</value>
    public UnityEvent PrintCompleted
    {
        get {
            return this.printCompleted;
        }
    }

    /// <summary>
    /// Gets the CharacterPrinted event, which includes a string for the character that was printed.
    /// </summary>
    /// <value>The character printed event.</value>
    public CharacterPrintedEvent CharacterPrinted
    {
        get {
            return this.characterPrinted;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="TextTyper"/> is currently printing text.
    /// </summary>
    /// <value><c>true</c> if printing; otherwise, <c>false</c>.</value>
    public bool IsTyping
    {
        get {
            return this.typeTextCoroutine != null;
        }
    }

    private Text TextComponent
    {
        get {
            if (this.textComponent == null)
            {
                this.textComponent = this.GetComponent<Text>();
            }

            return this.textComponent;
        }
    }

    /// <summary>
    /// Types the text into the Text component character by character, using the specified (optional) print delay per
    /// character.
    /// </summary>
    /// <param name="text">Text to type.</param>
    /// <param name="printDelay">Print delay (in seconds) per character.</param>
    public void TypeText(string text, float printDelay = -1)
    {
        this.Cleanup();

        this.defaultPrintDelay = printDelay > 0 ? printDelay : PrintDelaySetting;
        this.printingText = text;

        this.typeTextCoroutine = this.StartCoroutine(this.TypeTextCharByChar(text));
    }

    /// <summary>
    /// Skips the typing to the end.
    /// </summary>
    public void Skip()
    {
        this.Cleanup();

        var generator = new TypedTextGenerator();
        var typedText = generator.GetCompletedText(this.printingText);
        this.TextComponent.text = typedText.TextToPrint;

        this.OnTypewritingComplete();
    }

    /// <summary>
    /// Determines whether this instance is skippable.
    /// </summary>
    /// <returns><c>true</c> if this instance is skippable; otherwise, <c>false</c>.</returns>
    public bool IsSkippable()
    {
        // For now there's no way to configure this. Just make sure it's currently typing.
        return this.IsTyping;
    }

    private void Cleanup()
    {
        if (this.typeTextCoroutine != null)
        {
            this.StopCoroutine(this.typeTextCoroutine);
            this.typeTextCoroutine = null;
        }
    }

    private IEnumerator TypeTextCharByChar(string text)
    {
        this.TextComponent.text = string.Empty;

        var generator = new TypedTextGenerator();
        TypedTextGenerator.TypedText typedText;
        int printedCharCount = 0;
        do
        {
            typedText = generator.GetTypedTextAt(text, printedCharCount);
            this.TextComponent.text = typedText.TextToPrint;
            this.OnCharacterPrinted(typedText.LastPrintedChar.ToString());

            ++printedCharCount;

            var delay =
                typedText.Delay > 0 ? typedText.Delay : this.GetPrintDelayForCharacter(typedText.LastPrintedChar);
            yield return new WaitForSeconds(delay);
        } while (!typedText.IsComplete);

        this.typeTextCoroutine = null;
        this.OnTypewritingComplete();
    }

    private float GetPrintDelayForCharacter(char characterToPrint)
    {
        // Then get the default print delay for the current character
        float punctuationDelay = this.defaultPrintDelay * 8.0f;
        if (this.punctutationCharacters.Contains(characterToPrint))
        {
            return punctuationDelay;
        }
        else
        {
            return this.defaultPrintDelay;
        }
    }

    private void OnCharacterPrinted(string printedCharacter)
    {
        if (this.CharacterPrinted != null)
        {
            this.CharacterPrinted.Invoke(printedCharacter);
        }
    }

    private void OnTypewritingComplete()
    {
        if (this.PrintCompleted != null)
        {
            this.PrintCompleted.Invoke();
        }
    }

    /// <summary>
    /// Event that signals a Character has been printed to the Text component.
    /// </summary>
    [System.Serializable]
    public class CharacterPrintedEvent : UnityEvent<string>
    {
    }
}
/// <summary>
/// Typed text generator is used to create TypedText results given a text that should be printed one character
/// at a time, up to the specified character.
/// </summary>
public sealed class TypedTextGenerator
{
    private static readonly List<string> UnityTagTypes = new List<string> { "b", "i", "size", "color" };
    private static readonly List<string> CustomTagTypes = new List<string> { "delay" };

    /// <summary>
    /// Gets Completed TypedText from the specified text string.
    /// </summary>
    /// <returns>The completed text, as it should display in Unity.</returns>
    /// <param name="text">Text to complete.</param>
    public TypedText GetCompletedText(string text)
    {
        var printText = RemoveCustomTags(text);

        var typedText = new TypedText();
        typedText.TextToPrint = printText;
        typedText.Delay = 0.0f;
        typedText.LastPrintedChar = printText[printText.Length - 1];
        typedText.IsComplete = true;

        return typedText;
    }

    /// <summary>
    /// Gets the typed text at the specified visibleCharacterIndex. This is the text that should be written
    /// to the Text component.
    /// </summary>
    /// <returns>The <see cref="TypedText"/> generated at the specified visible character index.</returns>
    /// <param name="text">Text to parse.</param>
    /// <param name="visibleCharacterIndex">Visible character index (ignores tags).</param>
    public TypedText GetTypedTextAt(string text, int visibleCharacterIndex)
    {
        var textAsSymbolList = CreateSymbolListFromText(text);

        // Split the text into shown and hide strings based on the actual visible characters
        int printedCharCount = 0;
        var shownText = string.Empty;
        var hiddenText = string.Empty;
        var lastVisibleCharacter = char.MinValue;
        foreach (var symbol in textAsSymbolList)
        {
            if (printedCharCount <= visibleCharacterIndex)
            {
                shownText += symbol.Text;

                // Keep track of the visible characters that have been printed
                if (!symbol.IsTag)
                {
                    lastVisibleCharacter = symbol.Character.ToCharArray()[0];
                }
            }
            else
            {
                hiddenText += symbol.Text;
            }

            if (!symbol.IsTag)
            {
                printedCharCount++;
            }
        }

        var activeTags = GetActiveTagsInSymbolList(textAsSymbolList, visibleCharacterIndex);

        // Remove closing tags for active tags from hidden text (move to before color hide tag)
        foreach (var activeTag in activeTags)
        {
            hiddenText = RemoveFirstOccurance(hiddenText, activeTag.ClosingTagText);
        }

        // Remove all color tags from hidden text so that they don't cause it to be shown
        // ex: <color=clear>This should <color=red>be clear</color></color> will show 'be clear" in red
        hiddenText = RichTextTag.RemoveTagsFromString(hiddenText, "color");

        // Add the hidden text, provided there is text to hide
        if (!string.IsNullOrEmpty(hiddenText))
        {
            var hiddenTag = RichTextTag.ClearColorTag;
            hiddenText = hiddenText.Insert(0, hiddenTag.TagText);
            hiddenText = hiddenText.Insert(hiddenText.Length, hiddenTag.ClosingTagText);
        }

        // Add back in closing tags in reverse order
        for (int i = 0; i < activeTags.Count; ++i)
        {
            hiddenText = hiddenText.Insert(0, activeTags[i].ClosingTagText);
        }

        // Remove all custom tags since Unity will display them when printed (it doesn't recognize them as rich text
        // tags)
        var printText = shownText + hiddenText;
        foreach (var customTag in CustomTagTypes)
        {
            printText = RichTextTag.RemoveTagsFromString(printText, customTag);
        }

        // Calculate Delay, if active
        var delay = 0.0f;
        foreach (var activeTag in activeTags)
        {
            if (activeTag.TagType == "delay")
            {
                try
                {
                    delay = activeTag.IsOpeningTag ? float.Parse(activeTag.Parameter) : 0.0f;
                }
                catch (System.FormatException e)
                {
                    var warning = string.Format("TypedTextGenerator found Invalid paramter format in tag [{0}]. " +
                                                    "Parameter [{1}] does not parse to a float. Exception: {2}",
                                                activeTag, activeTag.Parameter, e);
                    Debug.Log(warning);
                    delay = 0.0f;
                }
            }
        }

        var typedText = new TypedText();
        typedText.TextToPrint = printText;
        typedText.Delay = delay;
        typedText.LastPrintedChar = lastVisibleCharacter;
        typedText.IsComplete = string.IsNullOrEmpty(hiddenText);

        return typedText;
    }

    private static List<RichTextTag> GetActiveTagsInSymbolList(List<TypedTextSymbol> symbolList,
                                                               int visibleCharacterPosition)
    {
        var activeTags = new List<RichTextTag>();
        int printableCharacterCount = 0;
        foreach (var symbol in symbolList)
        {
            if (symbol.IsTag)
            {
                if (symbol.Tag.IsOpeningTag)
                {
                    activeTags.Add(symbol.Tag);
                }
                else
                {
                    var poppedTag = activeTags[activeTags.Count - 1];
                    if (poppedTag.TagType != symbol.Tag.TagType)
                    {
                        var errorMessage = string.Format(
                            "TypedTextGenerator Popped TagType [{0}] that did not match last outstanding tagType [{1}]" +
                                ". Unity only respects tags that are added and closed as a stack.",
                            poppedTag.TagType, symbol.Tag.TagType);
                        Debug.LogError(errorMessage);
                    }

                    activeTags.RemoveAt(activeTags.Count - 1);
                }
            }
            else
            {
                printableCharacterCount++;

                // Finished when we've passed the visibleCharacter (non-Tag) position
                if (printableCharacterCount > visibleCharacterPosition)
                {
                    break;
                }
            }
        }

        return activeTags;
    }

    private static List<TypedTextSymbol> CreateSymbolListFromText(string text)
    {
        var symbolList = new List<TypedTextSymbol>();
        int parsedCharacters = 0;
        while (parsedCharacters < text.Length)
        {
            TypedTextSymbol symbol = null;

            // Check for tags
            var remainingText = text.Substring(parsedCharacters, text.Length - parsedCharacters);
            if (RichTextTag.StringStartsWithTag(remainingText))
            {
                var tag = RichTextTag.ParseNext(remainingText);
                symbol = new TypedTextSymbol(tag);
            }
            else
            {
                symbol = new TypedTextSymbol(remainingText.Substring(0, 1));
            }

            parsedCharacters += symbol.Length;
            symbolList.Add(symbol);
        }

        return symbolList;
    }

    private static char GetLastVisibleCharInSymbolList(List<TypedTextSymbol> symbolList)
    {
        for (int i = symbolList.Count - 1; i >= 0; --i)
        {
            var symbol = symbolList[i];
            if (!symbol.IsTag)
            {
                return symbol.Character.ToCharArray()[0];
            }
        }

        return char.MinValue;
    }

    private static string RemoveAllTags(string textWithTags)
    {
        string textWithoutTags = textWithTags;
        textWithoutTags = RemoveUnityTags(textWithoutTags);
        textWithoutTags = RemoveCustomTags(textWithoutTags);

        return textWithoutTags;
    }

    private static string RemoveCustomTags(string textWithTags)
    {
        string textWithoutTags = textWithTags;
        foreach (var customTag in CustomTagTypes)
        {
            textWithoutTags = RichTextTag.RemoveTagsFromString(textWithoutTags, customTag);
        }

        return textWithoutTags;
    }

    private static string RemoveUnityTags(string textWithTags)
    {
        string textWithoutTags = textWithTags;
        foreach (var unityTag in UnityTagTypes)
        {
            textWithoutTags = RichTextTag.RemoveTagsFromString(textWithoutTags, unityTag);
        }

        return textWithoutTags;
    }

    private static string RemoveFirstOccurance(string source, string searchString)
    {
        var index = source.IndexOf(searchString);
        if (index >= 0)
        {
            return source.Remove(index, searchString.Length);
        }
        else
        {
            return source;
        }
    }

    /// <summary>
    /// TypedText represents results from the TypedTextGenerator
    /// </summary>
    public class TypedText
    {
        /// <summary>
        /// Gets or sets the text to print to the Text component. This is what is visible to the user.
        /// </summary>
        /// <value>The text to print.</value>
        public string TextToPrint { get; set; }

        /// <summary>
        /// Gets or sets the desired Delay based on the delay tags in the typed text.
        /// </summary>
        /// <value>The delay.</value>
        public float Delay { get; set; }

        /// <summary>
        /// Gets or sets the last printed char.
        /// </summary>
        /// <value>The last printed char.</value>
        public char LastPrintedChar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is complete and has printed all its characters)
        /// </summary>
        /// <value><c>true</c> if this instance is complete; otherwise, <c>false</c>.</value>
        public bool IsComplete { get; set; }
    }

    private class TypedTextSymbol
    {
        public TypedTextSymbol(string character)
        {
            this.Character = character;
        }

        public TypedTextSymbol(RichTextTag tag)
        {
            this.Tag = tag;
        }

        public string Character { get; private set; }

        public RichTextTag Tag { get; private set; }

        public int Length
        {
            get {
                return this.Text.Length;
            }
        }

        public string Text
        {
            get {
                if (this.IsTag)
                {
                    return this.Tag.TagText;
                }
                else
                {
                    return this.Character;
                }
            }
        }

        public bool IsTag
        {
            get {
                return this.Tag != null;
            }
        }
    }
}
/// <summary>
/// RichTextTags help parse text that contains HTML style tags, used by Unity's RichText text components.
/// </summary>
public class RichTextTag
{
    public static readonly RichTextTag ClearColorTag = new RichTextTag("<color=#00000000>");

    private const char OpeningNodeDelimeter = '<';
    private const char CloseNodeDelimeter = '>';
    private const char EndTagDelimeter = '/';
    private const string ParameterDelimeter = "=";

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextTag"/> class.
    /// </summary>
    /// <param name="tagText">Tag text.</param>
    public RichTextTag(string tagText)
    {
        this.TagText = tagText;
    }

    /// <summary>
    /// Gets the full tag text including markers.
    /// </summary>
    /// <value>The tag full text.</value>
    public string TagText { get; private set; }

    /// <summary>
    /// Gets the text for this tag if it's used as a closing tag. Closing tags are unchanged.
    /// </summary>
    /// <value>The closing tag text.</value>
    public string ClosingTagText
    {
        get {
            return this.IsClosingTag ? this.TagText : string.Format("</{0}>", this.TagType);
        }
    }

    /// <summary>
    /// Gets the TagType, the body of the tag as a string
    /// </summary>
    /// <value>The type of the tag.</value>
    public string TagType
    {
        get {
            // Strip start and end tags
            var tagType = this.TagText.Substring(1, this.TagText.Length - 2);
            tagType = tagType.TrimStart(EndTagDelimeter);

            // Strip Parameter
            var parameterDelimeterIndex = tagType.IndexOf(ParameterDelimeter);
            if (parameterDelimeterIndex > 0)
            {
                tagType = tagType.Substring(0, parameterDelimeterIndex);
            }

            return tagType;
        }
    }

    /// <summary>
    /// Gets the parameter as a string. Ex: For tag Color=#FF00FFFF the parameter would be #FF00FFFF.
    /// </summary>
    /// <value>The parameter.</value>
    public string Parameter
    {
        get {
            var parameterDelimeterIndex = this.TagText.IndexOf(ParameterDelimeter);
            if (parameterDelimeterIndex < 0)
            {
                return string.Empty;
            }

            // Subtract two, one for the delimeter and one for the closing character
            var parameterLength = this.TagText.Length - parameterDelimeterIndex - 2;
            return this.TagText.Substring(parameterDelimeterIndex + 1, parameterLength);
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is an opening tag.
    /// </summary>
    /// <value><c>true</c> if this instance is an opening tag; otherwise, <c>false</c>.</value>
    public bool IsOpeningTag
    {
        get {
            return !this.IsClosingTag;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is a closing tag.
    /// </summary>
    /// <value><c>true</c> if this instance is a closing tag; otherwise, <c>false</c>.</value>
    public bool IsClosingTag
    {
        get {
            return this.TagText.Length > 2 && this.TagText[1] == EndTagDelimeter;
        }
    }

    /// <summary>
    /// Gets the length of the tag. Shorcut for the length of the full TagText.
    /// </summary>
    /// <value>The text length.</value>
    public int Length
    {
        get {
            return this.TagText.Length;
        }
    }

    /// <summary>
    /// Checks if the specified String starts with a tag.
    /// </summary>
    /// <returns><c>true</c>, if the first character begins a tag <c>false</c> otherwise.</returns>
    /// <param name="text">Text to check for tags.</param>
    public static bool StringStartsWithTag(string text)
    {
        return text.StartsWith(RichTextTag.OpeningNodeDelimeter.ToString());
    }

    /// <summary>
    /// Parses the text for the next RichTextTag.
    /// </summary>
    /// <returns>The next RichTextTag in the sequence. Null if the sequence contains no RichTextTag</returns>
    /// <param name="text">Text to parse.</param>
    public static RichTextTag ParseNext(string text)
    {
        // Trim up to the first delimeter
        var openingDelimeterIndex = text.IndexOf(RichTextTag.OpeningNodeDelimeter);

        // No opening delimeter found. Might want to throw.
        if (openingDelimeterIndex < 0)
        {
            return null;
        }

        var closingDelimeterIndex = text.IndexOf(RichTextTag.CloseNodeDelimeter);

        // No closingDelimeter found. Might want to throw.
        if (closingDelimeterIndex < 0)
        {
            return null;
        }

        var tagText = text.Substring(openingDelimeterIndex, closingDelimeterIndex - openingDelimeterIndex + 1);
        return new RichTextTag(tagText);
    }

    /// <summary>
    /// Removes all copies of the tag of the specified type from the text string.
    /// </summary>
    /// <returns>The text string without any tag of the specified type.</returns>
    /// <param name="text">Text to remove Tags from.</param>
    /// <param name="tagType">Tag type to remove.</param>
    public static string RemoveTagsFromString(string text, string tagType)
    {
        var bodyWithoutTags = text;
        for (int i = 0; i < text.Length; ++i)
        {
            var remainingText = text.Substring(i, text.Length - i);
            if (StringStartsWithTag(remainingText))
            {
                var parsedTag = ParseNext(remainingText);
                if (parsedTag.TagType == tagType)
                {
                    bodyWithoutTags = bodyWithoutTags.Replace(parsedTag.TagText, string.Empty);
                }

                i += parsedTag.Length - 1;
            }
        }

        return bodyWithoutTags;
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="RichTextTag"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="RichTextTag"/>.</returns>
    public override string ToString()
    {
        return this.TagText;
    }
}