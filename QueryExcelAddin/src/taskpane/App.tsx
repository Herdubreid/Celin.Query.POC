import React from "react";
import axios from "axios";
import {
  PrimaryButton,
  Stack,
  IStackTokens,
  Spinner,
  SpinnerSize,
  StackItem,
  MessageBar,
  MessageBarType,
} from "@fluentui/react";
import CqlEditor from "./components/CqlEditor";
import IQueryResponse from "./IQueryResponse";

export interface IAppProps {}
export interface IAppState {
  code: string;
  warning: string;
  error: string;
  busy: boolean;
}

export default class App extends React.Component<IAppProps, IAppState> {
  state = { code: "", warning: "", error: "", busy: false };
  click = async () => {
    this.setState({ warning: "", error: "", busy: true });
    try {
      await Excel.run(async (context) => {
        await axios({
          method: "POST",
          headers: { "Content-Type": "application/json" },
          data: JSON.stringify({
            code: this.state.code,
          }),
        })
          .then((res) => {
            const qry: IQueryResponse = res.data;
            if (qry.Error > "") {
              this.setState({ error: qry.Error });
            } else {
              if (qry.Summary.records > 0) {
                if (qry.Summary.moreRecords) {
                  this.setState({ warning: `Results limited to ${qry.Summary.records} rows.` });
                }
                const sheet = context.workbook.worksheets.add();
                const range = sheet.getRange("A1").getResizedRange(0, qry.Data[0].length - 1);
                const table = sheet.tables.add(range, true);
                const titles = Object.entries(qry.Columns).map((c) => (c[1] === "" ? c[0] : c[1]));
                table.getHeaderRowRange().values = [titles];
                table.rows.add(null, qry.Data);
                sheet.getUsedRange().format.autofitColumns();
                sheet.getUsedRange().format.autofitRows();
                sheet.activate();
              } else {
                this.setState({ warning: "No records returned!" });
              }
            }
          })
          .catch((err) => {
            console.error(err);
          });
        await context.sync();
      });
    } catch (error) {
      console.error(error);
    }
    this.setState({ busy: false });
  };
  handleCodeChange = (code: string) => this.setState({ code });
  stackStyle: IStackTokens = {
    childrenGap: 10,
  };
  render() {
    let spinner;
    if (this.state.busy) {
      spinner = <Spinner size={SpinnerSize.small} />;
    }
    let warning;
    if (this.state.warning > "") {
      warning = (
        <StackItem align="center">
          <MessageBar messageBarType={MessageBarType.warning}>{this.state.warning}</MessageBar>
        </StackItem>
      );
    }
    let error;
    if (this.state.error > "") {
      error = (
        <StackItem align="center">
          <MessageBar messageBarType={MessageBarType.error}>{this.state.error}</MessageBar>
        </StackItem>
      );
    }
    return (
      <Stack tokens={this.stackStyle}>
        <Stack.Item align="start">
          <PrimaryButton onClick={this.click} disabled={this.state.busy}>
            Run&nbsp;
            {spinner}
          </PrimaryButton>
        </Stack.Item>
        {warning}
        {error}
        <Stack.Item>
          <CqlEditor code={this.state.code} onValueChange={this.handleCodeChange} />
        </Stack.Item>
      </Stack>
    );
  }
}
