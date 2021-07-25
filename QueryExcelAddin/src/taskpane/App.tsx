import React from "react";
import axios from "axios";
import {
  Stack,
  IStackTokens,
  Spinner,
  SpinnerSize,
  StackItem,
  MessageBar,
  MessageBarType,
  IconButton,
  IIconProps,
} from "@fluentui/react";
import { CqlEditor } from "./components/cqlEditor";
import { IQueryResponse } from "./components/IQueryResponse";
import { PasteResult } from "./components/pasteResult";

interface IAppProps {}
interface IAppState {
  ids: number[];
  error: string;
  busy: boolean;
  result: IQueryResponse | null;
}

export default class App extends React.Component<IAppProps, IAppState> {
  state = {
    ids: [0],
    error: "",
    busy: false,
    result: null,
  };
  handleAdd = () => {
    this.setState({
      ids: [Math.max(...this.state.ids) + 1, ...this.state.ids],
    });
  };
  handleDel = (id: number) => {
    const ndx = this.state.ids.indexOf(id);
    this.setState({
      ids: [...this.state.ids.slice(0, ndx), ...this.state.ids.slice(ndx + 1, this.state.ids.length)],
    });
  };
  handleSub = async (code: string) => {
    let result: IQueryResponse | null = null;
    this.setState({ error: "", busy: true, result: result });
    try {
      await axios({
        method: "POST",
        headers: { "Content-Type": "application/json" },
        //url: "http://localhost:7071/api/Submit",
        url: "https://celinqueryfunctions.azurewebsites.net/api/Submit?code=hZgkPauoUXuLFOr2Tat3o6poXM7j0wnsObvoKCE8MPeKsvUF2N5tJg==",
        data: JSON.stringify({
          code,
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
  stackStyle: IStackTokens = {
    childrenGap: 10,
    padding: 5,
  };
  addIcon: IIconProps = { iconName: "Add" };
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
        {error}
        <Stack.Item align="start">
          <Stack horizontal>
            <IconButton onClick={this.handleAdd} iconProps={this.addIcon} title="Add Editor" />
            {spinner}
          </Stack>
        </Stack.Item>
        {this.state.ids.map((id) => (
          <Stack.Item key={id}>
            <CqlEditor id={id} code="" onSub={this.handleSub} onDel={this.handleDel} busy={this.state.busy} />
          </Stack.Item>
        ))}
      </Stack>
    );
  }
}
