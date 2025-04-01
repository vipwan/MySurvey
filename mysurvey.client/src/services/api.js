import axios from 'axios';

const API_BASE_URL = process.env.NODE_ENV === 'production' ? '/' : 'http://localhost:5289';

const api = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json'
    },
});

// 请求拦截器
api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

// 响应拦截器
api.interceptors.response.use(
    (response) => response,
    async (error) => {
        // 详细记录错误信息，帮助调试
        console.log('API Error Object:', error);

        // 检查错误类型并处理
        if (error.name === 'AxiosError') {
            console.log('Axios Error Type:', error.name);
            console.log('Error Message:', error.message);
            console.log('Error Code:', error.code);
            console.log('Error Status:', error.response?.status);

            // 处理网络错误
            if (error.code === 'ERR_NETWORK') {
                console.log('Network error detected');
                // 可以在这里显示网络错误提示
            }

            // 处理401未授权错误 - 不同错误模式的处理
            if (
                // 正常响应中的401状态
                (error.response && error.response.status === 401) ||
                // 特定错误代码（可能没有response对象）
                error.message.includes('401') ||
                // 检查是否为"Unauthorized"错误消息
                (error.response?.data &&
                    (error.response.data.includes('Unauthorized') ||
                        error.response.data.title === 'Unauthorized'))
            ) {
                console.log('Unauthorized error detected, redirecting to login');

                // 清除认证信息
                localStorage.removeItem('token');
                localStorage.removeItem('user');

                // 避免在已经在登录页时重定向
                if (window.location.pathname !== '/login') {
                    // 使用history API保留导航历史
                    window.location.href = '/login';
                }
            }
        }

        return Promise.reject(error);
    }
);

// 认证相关API
export const authApi = {
    login: (data) => api.post('/account/login', data),
    register: (data) => api.post('/account/register', data),
    refreshToken: (data) => api.post('/account/refresh', data),
    confirmEmail: (params) => api.get('/account/confirmEmail', { params }),
    resendConfirmationEmail: (data) => api.post('/account/resendConfirmationEmail', data),
    forgotPassword: (data) => api.post('/account/forgotPassword', data),
    resetPassword: (data) => api.post('/account/resetPassword', data),
    getInfo: () => api.get('/account/manage/info'),
    updateInfo: (data) => api.post('/account/manage/info', data),
};

// 问卷相关API
export const surveyApi = {

    // 获取问题列表（通过问卷详情接口）
    getQuestions: async (surveyId) => {
        const response = await api.get(`/api/surveys/${surveyId}`);
        return { data: response.data.questions || [] };
    },
    // 获取问卷列表
    getSurveys: (params) => api.get('/api/surveys', { params }),
    // 获取公开问卷列表
    getPublicSurveys: (params) => api.get('/api/public-surveys', { params }),
    // 获取问卷详情
    getSurvey: (id) => api.get(`/api/surveys/${id}`),
    // 创建问卷
    createSurvey: (data) => api.post('/api/surveys', data),
    // 更新问卷
    updateSurvey: (id, data) => api.put(`/api/surveys/${id}`, data),
    // 删除问卷
    deleteSurvey: (id) => api.delete(`/api/surveys/${id}`),
    // 发布问卷
    publishSurvey: (id) => api.post(`/api/surveys/${id}/publish`),
    // 结束问卷
    endSurvey: (id) => api.post(`/api/surveys/${id}/end`),
    // 复制问卷
    copySurvey: (id) => api.post(`/api/surveys/${id}/copy`),

    // 将问卷设置为草稿状态
    setToDraft: (id) => api.post(`/api/surveys/${id}/draft`),

    // 添加问题
    addQuestion: (surveyId, data) => api.post(`/api/surveys/${surveyId}/questions`, data),
    // 更新问题
    updateQuestion: (surveyId, questionId, data) =>
        api.put(`/api/surveys/${surveyId}/questions/${questionId}`, data),
    // 删除问题
    deleteQuestion: (surveyId, questionId) =>
        api.delete(`/api/surveys/${surveyId}/questions/${questionId}`),
    // 提交答案
    submitAnswer: (surveyId, data) => api.post(`/api/surveys/${surveyId}/submit`, data),
    // 获取问卷答案列表
    getAnswers: (surveyId) => api.get(`/api/surveys/${surveyId}/answers`),
    // 获取答案详情
    getAnswer: (surveyId, answerId) => api.get(`/api/surveys/${surveyId}/answers/${answerId}`),

    // 导出问卷答卷
    exportAnswers: (surveyId) => api.get(`/api/surveys/${surveyId}/export`, {
        responseType: 'blob'
    }),

    // 统计数据
    stat: () => api.get(`/api/dashboard-data`),
};

// 站点设置API
export const siteSettingApi = {
    getSiteSetting: () => api.get('/api/siteSetting'),
};

export default api;