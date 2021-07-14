import React from "react";
import {
  Stack,
  StackItem,
  IStackTokens,
  MessageBar,
  MessageBarType,
  Checkbox,
  IconButton,
  IIconProps,
} from "@fluentui/react";
import { IQueryResponse } from "./IQueryResponse";

interface IPasteResultProps {
  result: IQueryResponse | null;
}
interface IPasteResultState {
  includeHeader: boolean;
}

export class PasteResult extends React.Component<IPasteResultProps, IPasteResultState> {
  tokens: IStackTokens = {
    childrenGap: 5,
  };
  state = {
    includeHeader: true,
  };
  pasteIcon: IIconProps = { iconName: "Paste" };
  insIcon: IIconProps = { iconName: "Insert" };
  handleIncludeHeader = () => this.setState({ includeHeader: !this.state.includeHeader });
  handlePaste = async () => {
    try {
      // eslint-disable-next-line no-undef
      await Excel.run(async (context) => {
        let startCell = context.workbook.getActiveCell();
        if (this.state.includeHeader) {
          const header = startCell.getResizedRange(0, this.props.result.Data[0].length - 1);
          const titles = Object.entries(this.props.result.Columns).map((c) => (c[1] === "" ? c[0] : c[1]));
          header.values = [titles];
          startCell = startCell.getCell(1, 0);
        }
        const detail = startCell.getResizedRange(
          this.props.result.Data.length - 1,
          this.props.result.Data[0].length - 1
        );
        detail.values = this.props.result.Data;
        await context.sync();
      });
      // eslint-disable-next-line no-empty
    } catch {}
  };
  handleInsert = async () => {
    try {
      // eslint-disable-next-line no-undef
      await Excel.run(async (context) => {
        const sheet = context.workbook.worksheets.add();
        const range = sheet.getRange("A1").getResizedRange(0, this.props.result.Data[0].length - 1);
        const table = sheet.tables.add(range, this.state.includeHeader);
        if (this.state.includeHeader) {
          const titles = Object.entries(this.props.result.Columns).map((c) => (c[1] === "" ? c[0] : c[1]));
          table.getHeaderRowRange().values = [titles];
        }
        table.rows.add(null, this.props.result.Data);
        sheet.getUsedRange().format.autofitColumns();
        sheet.getUsedRange().format.autofitRows();
        sheet.activate();
        await context.sync();
      });
      // eslint-disable-next-line no-empty
    } catch {}
  };
  render() {
    let paste = <StackItem />;
    let warning;
    if (this.props.result !== null) {
      if (this.props.result.Summary.moreRecords) {
        warning = (
          <StackItem align="start">
            <MessageBar messageBarType={MessageBarType.warning}>Max records returned!</MessageBar>
          </StackItem>
        );
      }
      if (this.props.result.Summary.records > 0) {
        paste = (
          <Stack tokens={this.tokens}>
            <StackItem align="start">
              <MessageBar messageBarType={MessageBarType.info}>
                Returned {this.props.result.Summary.records} row(s) with {this.props.result.Data[0].length} column(s)
              </MessageBar>
            </StackItem>
            <Stack horizontal tokens={this.tokens}>
              <IconButton onClick={this.handlePaste} title="Past Result from Active Cell" iconProps={this.pasteIcon} />
              <IconButton onClick={this.handleInsert} title="Insert Result as Table" iconProps={this.insIcon} />
              <Stack.Item align="center">
                <Checkbox
                  label="Include Header"
                  boxSide="end"
                  onChange={this.handleIncludeHeader}
                  checked={this.state.includeHeader}
                />
              </Stack.Item>
            </Stack>
            {warning}
          </Stack>
        );
      } else {
        paste = (
          <StackItem align="start">
            <MessageBar messageBarType={MessageBarType.warning}>Zero records returned!</MessageBar>
          </StackItem>
        );
      }
    }
    return paste;
  }
}
