import React, { Fragment } from "react"
import { render } from "react-dom"
import { AppContainer as ReactHotAppContainer } from "react-hot-loader"
import GPS from "./components/GPS"
import Spectrometer from "./components/Spectrometer"

const AppContainer = process.env.PLAIN_HMR ? Fragment : ReactHotAppContainer

document.addEventListener("DOMContentLoaded", () => {
  // eslint-disable-next-line global-require
  // const Root = require('./containers/Root').default
  render(
    <AppContainer>
      <div>
        <Spectrometer />
      </div>
    </AppContainer>,
    document.getElementById("root")
  )
})
