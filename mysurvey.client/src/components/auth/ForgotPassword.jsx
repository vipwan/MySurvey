import React, { useState } from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { MailOutlined } from '@ant-design/icons';
import { Link } from 'react-router-dom';
import { authApi } from '../../services/api';

const ForgotPassword = () => {
    const [loading, setLoading] = useState(false);

    const onFinish = async (values) => {
        try {
            setLoading(true);
            await authApi.forgotPassword(values);
            message.success('重置密码链接已发送到您的邮箱');
        } catch (error) {
            message.error(error.response?.data?.detail || '发送失败');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            background: '#f0f2f5'
        }}>
            <Card title="忘记密码" style={{ width: 400 }}>
                <Form
                    name="forgotPassword"
                    onFinish={onFinish}
                    autoComplete="off"
                >
                    <Form.Item
                        name="email"
                        rules={[
                            { required: true, message: '请输入邮箱' },
                            { type: 'email', message: '请输入有效的邮箱地址' }
                        ]}
                    >
                        <Input
                            prefix={<MailOutlined />}
                            placeholder="邮箱"
                            size="large"
                        />
                    </Form.Item>

                    <Form.Item>
                        <Button
                            type="primary"
                            htmlType="submit"
                            loading={loading}
                            block
                            size="large"
                        >
                            发送重置链接
                        </Button>
                    </Form.Item>

                    <div style={{ textAlign: 'center' }}>
                        <Link to="/login">返回登录</Link>
                    </div>
                </Form>
            </Card>
        </div>
    );
};

export default ForgotPassword; 