import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { ConfigProvider, App as AntdApp } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import store from './store';
import MainLayout from './layouts/MainLayout';
import Login from './components/auth/Login';
import Register from './components/auth/Register';
import ForgotPassword from './components/auth/ForgotPassword';
import ResetPassword from './components/auth/ResetPassword';
import SurveyList from './components/survey/SurveyList';
import SurveyEdit from './components/survey/SurveyEdit';
import SurveyPreview from './components/survey/SurveyPreview';
import SurveyAnswer from './components/survey/SurveyAnswer';
import SurveySuccess from './components/survey/SurveySuccess';
import QuestionList from './components/survey/QuestionList';
import Dashboard from './components/dashboard/Dashboard';
import About from './components/about/About';
import AnonymousSurvey from './components/survey/AnonymousSurvey';
import SystemSetting from './components/dashboard/SystemSetting';

import './App.css';

// 路由守卫组件
const PrivateRoute = ({ children }) => {
    const token = localStorage.getItem('token');
    return token ? <MainLayout>{children}</MainLayout> : <Navigate to="/login" />;
};

function App() {
    return (
        <Provider store={store}>
            <ConfigProvider locale={zhCN}>
                <AntdApp>
                    <Router>
                        <Routes>
                            {/* 公开路由 */}
                            <Route path="/login" element={<Login />} />
                            <Route path="/register" element={<Register />} />
                            <Route path="/forgot-password" element={<ForgotPassword />} />
                            <Route path="/reset-password" element={<ResetPassword />} />
                            <Route path="/anonymous-survey/:id" element={<AnonymousSurvey />} />
                            <Route path="/survey-success" element={<SurveySuccess />} />

                            {/* 需要认证的路由 */}
                            <Route
                                path="/dashboard"
                                element={
                                    <PrivateRoute>
                                        <Dashboard />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/surveys"
                                element={
                                    <PrivateRoute>
                                        <SurveyList />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/surveys/create"
                                element={
                                    <PrivateRoute>
                                        <SurveyEdit />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/surveys/:id/edit"
                                element={
                                    <PrivateRoute>
                                        <SurveyEdit />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/surveys/:id/preview"
                                element={
                                    <PrivateRoute>
                                        <SurveyPreview />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/surveys/:id/questions"
                                element={
                                    <PrivateRoute>
                                        <QuestionList />
                                    </PrivateRoute>
                                }
                            />
                            <Route
                                path="/profile"
                                element={
                                    <PrivateRoute>
                                        <div>个人设置页面(待实现)</div>
                                    </PrivateRoute>
                                }
                            />

                            <Route
                                path="/about"
                                element={
                                    <PrivateRoute>
                                        <About />
                                    </PrivateRoute>
                                }
                            />

                            <Route
                                path="/system-settings"
                                element={
                                    <PrivateRoute>
                                        <SystemSetting />
                                    </PrivateRoute>
                                }
                            />

                            {/* 默认重定向到仪表板 */}
                            <Route path="/" element={<Navigate to="/login" replace />} />
                        </Routes>
                    </Router>
                </AntdApp>
            </ConfigProvider>
        </Provider>
    );
}

export default App;
