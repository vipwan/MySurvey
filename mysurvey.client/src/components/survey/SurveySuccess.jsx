import React from 'react';
import { Result, Button } from 'antd';
import { useNavigate } from 'react-router-dom';

const SurveySuccess = () => {
    const navigate = useNavigate();

    return (
        <Result
            status="success"
            title="提交成功"
            subTitle="感谢您的参与！"
            extra={[
                <Button type="primary" key="home" onClick={() => navigate('/')}>
                    返回首页
                </Button>,
            ]}
        />
    );
};

export default SurveySuccess; 