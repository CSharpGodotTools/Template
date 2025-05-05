using Godot;

namespace Template;

[InputMap]
public static partial class InputActions
{
    // Source generator generates the inputs in different partial
    // Use like so: InputActions.MoveLeft, InputActions.MoveRight, etc

    // Built-In Actions
    // Source: https://gist.github.com/qwe321qwe321qwe321/bbf4b135c49372746e45246b364378c4
    public static readonly StringName UIAccept = "ui_accept";
    public static readonly StringName UISelect = "ui_select";
    public static readonly StringName UICancel = "ui_cancel";
    public static readonly StringName UIFocusNext = "ui_focus_next";
    public static readonly StringName UIFocusPrev = "ui_focus_prev";
    public static readonly StringName UILeft = "ui_left";
    public static readonly StringName UIRight = "ui_right";
    public static readonly StringName UIUp = "ui_up";
    public static readonly StringName UIDown = "ui_down";
    public static readonly StringName UIPageUp = "ui_page_up";
    public static readonly StringName UIPageDown = "ui_page_down";
    public static readonly StringName UIHome = "ui_home";
    public static readonly StringName UIEnd = "ui_end";
    public static readonly StringName UICut = "ui_cut";
    public static readonly StringName UICopy = "ui_copy";
    public static readonly StringName UIPaste = "ui_paste";
    public static readonly StringName UIUndo = "ui_undo";
    public static readonly StringName UIRedo = "ui_redo";
    public static readonly StringName UITextCompletionQuery = "ui_text_completion_query";
    public static readonly StringName UITextNewline = "ui_text_newline";
    public static readonly StringName UITextNewlineBlank = "ui_text_newline_blank";
    public static readonly StringName UITextNewlineAbove = "ui_text_newline_above";
    public static readonly StringName UITextIndent = "ui_text_indent";
    public static readonly StringName UITextDedent = "ui_text_dedent";
    public static readonly StringName UITextBackspace = "ui_text_backspace";
    public static readonly StringName UITextBackspaceWord = "ui_text_backspace_word";
    public static readonly StringName UITextBackspaceWordMacos = "ui_text_backspace_word.macos";
    public static readonly StringName UITextBackspaceAllToLeft = "ui_text_backspace_all_to_left";
    public static readonly StringName UITextBackspaceAllToLeftMacos = "ui_text_backspace_all_to_left.macos";
    public static readonly StringName UITextDelete = "ui_text_delete";
    public static readonly StringName UITextDeleteWord = "ui_text_delete_word";
    public static readonly StringName UITextDeleteWordMacos = "ui_text_delete_word.macos";
    public static readonly StringName UITextDeleteAllToRight = "ui_text_delete_all_to_right";
    public static readonly StringName UITextDeleteAllToRightMacos = "ui_text_delete_all_to_right.macos";
    public static readonly StringName UITextCaretLeft = "ui_text_caret_left";
    public static readonly StringName UITextCaretWordLeft = "ui_text_caret_word_left";
    public static readonly StringName UITextCaretWordLeftMacos = "ui_text_caret_word_left.macos";
    public static readonly StringName UITextCaretRight = "ui_text_caret_right";
    public static readonly StringName UITextCaretWordRight = "ui_text_caret_word_right";
    public static readonly StringName UITextCaretWordRightMacos = "ui_text_caret_word_right.macos";
    public static readonly StringName UITextCaretUp = "ui_text_caret_up";
    public static readonly StringName UITextCaretDown = "ui_text_caret_down";
    public static readonly StringName UITextCaretLineStart = "ui_text_caret_line_start";
    public static readonly StringName UITextCaretLineStartMacos = "ui_text_caret_line_start.macos";
    public static readonly StringName UITextCaretLineEnd = "ui_text_caret_line_end";
    public static readonly StringName UITextCaretLineEndMacos = "ui_text_caret_line_end.macos";
    public static readonly StringName UITextCaretPageUp = "ui_text_caret_page_up";
    public static readonly StringName UITextCaretPageDown = "ui_text_caret_page_down";
    public static readonly StringName UITextCaretDocumentStart = "ui_text_caret_document_start";
    public static readonly StringName UITextCaretDocumentStartMacos = "ui_text_caret_document_start.macos";
    public static readonly StringName UITextCaretDocumentEnd = "ui_text_caret_document_end";
    public static readonly StringName UITextCaretDocumentEndMacos = "ui_text_caret_document_end.macos";
    public static readonly StringName UITextCaretAddBelow = "ui_text_caret_add_below";
    public static readonly StringName UITextCaretAddBelowMacos = "ui_text_caret_add_below.macos";
    public static readonly StringName UITextCaretAddAbove = "ui_text_caret_add_above";
    public static readonly StringName UITextCaretAddAboveMacos = "ui_text_caret_add_above.macos";
    public static readonly StringName UITextScrollUp = "ui_text_scroll_up";
    public static readonly StringName UITextScrollUpMacos = "ui_text_scroll_up.macos";
    public static readonly StringName UITextScrollDown = "ui_text_scroll_down";
    public static readonly StringName UITextScrollDownMacos = "ui_text_scroll_down.macos";
    public static readonly StringName UITextSelectAll = "ui_text_select_all";
    public static readonly StringName UITextSelectWordUnderCaret = "ui_text_select_word_under_caret";
    public static readonly StringName UITextAddSelectionForNextOccurrence = "ui_text_add_selection_for_next_occurrence";
    public static readonly StringName UITextSkipSelectionForNextOccurrence = "ui_text_skip_selection_for_next_occurrence";
    public static readonly StringName UITextClearCaretsAndSelection = "ui_text_clear_carets_and_selection";
    public static readonly StringName UITextToggleInsertMode = "ui_text_toggle_insert_mode";
    public static readonly StringName UITextSubmit = "ui_text_submit";
    public static readonly StringName UIGraphDuplicate = "ui_graph_duplicate";
    public static readonly StringName UIGraphDelete = "ui_graph_delete";
    public static readonly StringName UIFiledialogUpOneLevel = "ui_filedialog_up_one_level";
    public static readonly StringName UIFiledialogRefresh = "ui_filedialog_refresh";
    public static readonly StringName UIFiledialogShowHidden = "ui_filedialog_show_hidden";
    public static readonly StringName UISwapInputDirection = "ui_swap_input_direction";
}
