// images references in the manifest
import "../../assets/icon-16.png";
import "../../assets/icon-32.png";
import "../../assets/icon-80.png";
import "../../assets/logo-filled.png";
//
import "prismjs/themes/prism.css";
//
import { AppContainer } from "react-hot-loader";
import { initializeIcons, ThemeProvider } from "@fluentui/react";
import React from "react";
import ReactDOM from "react-dom";
/* global document, Office, module, require */
import App from "./App";

initializeIcons();

let isOfficeInitialized = false;

const title = "CelinQL Task Pane Add-in";

const render = (Component) => {
  ReactDOM.render(
    <AppContainer>
      <ThemeProvider>
        <Component title={title} isOfficeInitialized={isOfficeInitialized} />
      </ThemeProvider>
    </AppContainer>,
    document.getElementById("container")
  );
};

/* Render application after Office initializes */
Office.initialize = () => {
  isOfficeInitialized = true;
  render(App);
};

if ((module as any).hot) {
  (module as any).hot.accept("./App", () => {
    const NextApp = require("./App").default;
    render(NextApp);
  });
}
