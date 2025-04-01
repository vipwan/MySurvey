import React, { useState, useRef } from 'react';
import { Form, Input, Button, Card, message, Alert } from 'antd';
import { UserOutlined, LockOutlined } from '@ant-design/icons';
import { useDispatch } from 'react-redux';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../../services/api';
import { loginSuccess } from '../../store/authSlice';

const Login = () => {
    const [loading, setLoading] = useState(false);
    const dispatch = useDispatch();
    const navigate = useNavigate();
    const [form] = Form.useForm();

    const onFinish = async (values) => {
        try {
            setLoading(true);
            const response = await authApi.login(values);
            dispatch(loginSuccess(response.data));
            message.success('登录成功');
            navigate('/dashboard');
        } catch (error) {
            message.error(error.response?.data?.detail || '登录失败');
        } finally {
            setLoading(false);
        }
    };

    const fillDemoAccount = () => {
        form.setFieldsValue({
            email: 'vipwan@sina.com',
            password: '123456'
        });
    };

    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '80vh'
        }}>
            <Card title="登录" style={{ width: 400 }}>
                <Form
                    name="login"
                    form={form}
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
                            prefix={<UserOutlined />}
                            placeholder="邮箱"
                            size="large"
                        />
                    </Form.Item>

                    <Form.Item
                        name="password"
                        rules={[{ required: true, message: '请输入密码' }]}
                    >
                        <Input.Password
                            prefix={<LockOutlined />}
                            placeholder="密码"
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
                            登录
                        </Button>
                    </Form.Item>

                    <div style={{ textAlign: 'center' }}>
                        <Link to="/register">注册账号</Link>
                        <span style={{ margin: '0 8px' }}>|</span>
                        <Link to="/forgot-password">忘记密码？</Link>
                    </div>
                </Form>

                <Alert
                    message="示例账号"
                    description={
                        <div>
                            邮箱: vipwan@sina.com<br />
                            密码: 123456<br />
                            <Button type="link" size="small" onClick={fillDemoAccount} style={{ padding: 0 }}>
                                一键填充
                            </Button>
                        </div>
                    }
                    type="info"
                    style={{ marginTop: 16 }}
                />
            </Card>
        </div>
    );
};

export default Login;
