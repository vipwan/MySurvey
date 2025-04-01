import React, { useState } from 'react';
import { Form, Input, Button, Card, message } from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authApi } from '../../services/api';

const ResetPassword = () => {
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const email = searchParams.get('email');
    const resetCode = searchParams.get('code');

    const onFinish = async (values) => {
        try {
            setLoading(true);
            await authApi.resetPassword({
                email,
                resetCode,
                newPassword: values.newPassword
            });
            message.success('密码重置成功，请登录');
            navigate('/login');
        } catch (error) {
            message.error(error.response?.data?.detail || '密码重置失败');
        } finally {
            setLoading(false);
        }
    };

    if (!email || !resetCode) {
        return (
            <div style={{
                display: 'flex',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: '100vh',
                background: '#f0f2f5'
            }}>
                <Card title="无效的链接" style={{ width: 400 }}>
                    <p>重置密码链接无效或已过期。</p>
                    <Button type="primary" onClick={() => navigate('/forgot-password')}>
                        重新发送重置链接
                    </Button>
                </Card>
            </div>
        );
    }

    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            background: '#f0f2f5'
        }}>
            <Card title="重置密码" style={{ width: 400 }}>
                <Form
                    name="resetPassword"
                    onFinish={onFinish}
                    autoComplete="off"
                >
                    <Form.Item
                        name="newPassword"
                        rules={[
                            { required: true, message: '请输入新密码' },
                            { min: 6, message: '密码长度不能小于6位' }
                        ]}
                    >
                        <Input.Password
                            prefix={<LockOutlined />}
                            placeholder="新密码"
                            size="large"
                        />
                    </Form.Item>

                    <Form.Item
                        name="confirmPassword"
                        dependencies={['newPassword']}
                        rules={[
                            { required: true, message: '请确认新密码' },
                            ({ getFieldValue }) => ({
                                validator(_, value) {
                                    if (!value || getFieldValue('newPassword') === value) {
                                        return Promise.resolve();
                                    }
                                    return Promise.reject(new Error('两次输入的密码不一致'));
                                },
                            }),
                        ]}
                    >
                        <Input.Password
                            prefix={<LockOutlined />}
                            placeholder="确认新密码"
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
                            重置密码
                        </Button>
                    </Form.Item>
                </Form>
            </Card>
        </div>
    );
};

export default ResetPassword; 