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
import IQueryResponse from "./components/IQueryResponse";
import CqlEditor from "./components/CqlEditor";
import { PasteResult } from "./components/pasteResult";

export interface IAppProps {}
export interface IAppState {
  code: string;
  error: string;
  busy: boolean;
  result: IQueryResponse | null;
}

export default class App extends React.Component<IAppProps, IAppState> {
  state = { code: "", warning: "", error: "", busy: false, result: null };
  click = async () => {
    let result: IQueryResponse | null = null;
    this.setState({ error: "", busy: true, result: result });
    try {
      await axios({
        method: "POST",
        headers: { "Content-Type": "application/json" },
        url: "https://celinqueryfunctions.azurewebsites.net/api/Submit?code=ucXWB4Lf4JAYtUSexBwtzKiEZYtumi16SaozU5yxyXP3Easoe6VzqA==",
        data: JSON.stringify({
          code: this.state.code,
        }),
      })
        .then(async (res) => {
          if (res.data.Error > "") {
            this.setState({ error: res.data.Error });
          } else {
            if (res.data.Demo) {
              // eslint-disable-next-line no-undef
              await Excel.run(async (context) => {
                const sheet = context.workbook.worksheets.add();
                const range = sheet.getRange("A1").getResizedRange(0, res.data.Data[0].length - 1);
                const table = sheet.tables.add(range, true);
                const titles = Object.entries(res.data.Columns).map((c) => (c[1] === "" ? c[0] : c[1]));
                table.getHeaderRowRange().values = [titles];
                table.rows.add(null, res.data.Data);
                sheet.getUsedRange().format.autofitColumns();
                sheet.getUsedRange().format.autofitRows();
                sheet.activate();
                await context.sync();
              });
            } else {
              result = res.data;
            }
          }
        })
        .catch((error) => {
          this.setState({ error });
        });
    } catch (error) {
      this.setState({ error });
    }
    this.setState({ busy: false, result: result });
  };
  handleCodeChange = (code: string) => this.setState({ code });
  stackStyle: IStackTokens = {
    childrenGap: 10,
    padding: 5,
  };
  render() {
    let spinner;
    if (this.state.busy) {
      spinner = <Spinner size={SpinnerSize.small} />;
    }
    let error;
    if (this.state.error > "") {
      error = (
        <StackItem align="start">
          <MessageBar messageBarType={MessageBarType.error}>{this.state.error}</MessageBar>
        </StackItem>
      );
    }
    return (
      <Stack tokens={this.stackStyle}>
        <PasteResult result={this.state.result} />
        <Stack.Item align="start">
          <PrimaryButton onClick={this.click} disabled={this.state.busy}>
            Run&nbsp;
            {spinner}
          </PrimaryButton>
        </Stack.Item>
        {error}
        <Stack.Item>
          <CqlEditor code={this.state.code} onValueChange={this.handleCodeChange} />
        </Stack.Item>
      </Stack>
    );
  }
}
