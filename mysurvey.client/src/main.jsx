/*
 * @Author: 万雅虎
 * @Date: 2025-03-29 13:49:50
 * @LastEditTime: 2025-03-31 02:21:55
 * @LastEditors: 万雅虎
 * @Description: 
 * @FilePath: \MySurvey\mysurvey.client\src\main.jsx
 * vipwan@sina.com © 万雅虎
 */
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'

//import './index.css'
import App from './App.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <App />
  </StrictMode>,
)
