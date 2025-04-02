import React, { useState, useEffect } from 'react';
import {
    Alert,
    Card,
    List,
    Typography,
    Button,
    message,
    Drawer,
    Form,
    Input,
    Space,
    Divider,
    Skeleton,
    Tag
} from 'antd';
import { EditOutlined, SaveOutlined, CloseOutlined, ReloadOutlined } from '@ant-design/icons';
import axios from 'axios';

const { Text, Paragraph } = Typography;
const { TextArea } = Input;

// 定义API基础URL
const API_BASE_URL = process.env.NODE_ENV === 'production' ? '/' : 'http://localhost:5289';

const SystemSetting = () => {
    const [settings, setSettings] = useState([]);
    const [loading, setLoading] = useState(true);
    const [currentSetting, setCurrentSetting] = useState(null);
    const [drawerVisible, setDrawerVisible] = useState(false);
    const [form] = Form.useForm();

    // 用于格式化JSON字符串的函数
    const formatJSON = (jsonString) => {
        try {
            const obj = JSON.parse(jsonString);
            return JSON.stringify(obj, null, 2);
        } catch (e) {
            // 如果解析失败，返回原始字符串
            console.log('JSON格式化失败', e);
            return jsonString;
        }
    };

    // 加载所有设置
    const fetchSettings = async () => {
        setLoading(true);
        try {
            const token = localStorage.getItem('token');
            const response = await axios.get(`${API_BASE_URL}/biwensetting/api/all`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response && response.data) {
                setSettings(response.data);
            }
        } catch (error) {
            console.error('获取设置失败', error);
            message.error('获取设置失败，请检查网络连接或登录状态');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchSettings();
    }, []);

    // 打开抽屉编辑器，并加载特定设置的详细信息
    const showDrawer = async (settingType, settingName) => {
        try {
            setLoading(true);
            const token = localStorage.getItem('token');
            const id = `${settingType}`;
            const response = await axios.get(`${API_BASE_URL}/biwensetting/api/get/${id}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response && response.data) {
                setCurrentSetting(response.data);

                // 格式化JSON
                const formattedContent = formatJSON(response.data.settingContent || '{}');

                form.setFieldsValue({
                    settingContent: formattedContent
                });

                setDrawerVisible(true);
            }
        } catch (error) {
            console.error('获取设置详情失败', error);
            message.error('获取设置详情失败');
        } finally {
            setLoading(false);
        }
    };

    // 关闭抽屉
    const closeDrawer = () => {
        setDrawerVisible(false);
        setCurrentSetting(null);
        form.resetFields();
    };

    // 提交表单，更新设置
    const onFinish = async (values) => {
        if (!currentSetting) return;
        try {
            setLoading(true);
            const token = localStorage.getItem('token');
            const id = `${currentSetting.settingType}`;

            // 尝试解析JSON字符串内容
            let contentToSend;
            try {
                contentToSend = JSON.parse(values.settingContent);
            } catch (error) {
                console.error('JSON解析错误', error);
                message.error('设置内容不是有效的JSON格式');
                setLoading(false);
                return;
            }

            console.log('即将发送请求:', {
                url: `${API_BASE_URL}/biwensetting/api/set/${id}`,
                data: contentToSend,
                headers: {
                    'Authorization': 'Bearer ***',  // 隐藏实际token
                    'Content-Type': 'application/json-patch+json'
                }
            });

            const response = await axios.post(
                `${API_BASE_URL}/biwensetting/api/set/${id}`,
                contentToSend,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json-patch+json'
                    }
                }
            );

            console.log('请求成功，响应:', response);

            message.success('设置更新成功');
            setDrawerVisible(false);
            // 刷新设置列表
            fetchSettings();
        } catch (error) {
            console.error('更新设置失败:', error);
            console.error('错误响应:', error.response?.data);
            message.error(`更新设置失败: ${error.response?.data || error.message}`);
        } finally {
            setLoading(false);
        }
    };

    // 格式化JSON按钮的处理函数
    const handleFormatJSON = () => {
        const currentContent = form.getFieldValue('settingContent');
        if (!currentContent) return;

        const formattedContent = formatJSON(currentContent);
        form.setFieldsValue({
            settingContent: formattedContent
        });
    };

    // 渲染设置列表
    return (
        <Card
            title="系统设置"
            extra={
                <Button
                    type="primary"
                    icon={<ReloadOutlined />}
                    onClick={fetchSettings}
                    loading={loading}
                >
                    刷新
                </Button>
            }
        >
            <Skeleton loading={loading} active paragraph={{ rows: 10 }}>
                <Alert
                    showIcon
                    description="当前仅作为演示功能,因此没有做权限控制,所有登录账号都可以修改系统配置,如果需要了解更多请移步Biwen.Settings仓储!"
                    type="warning"
                />
                <List
                    itemLayout="horizontal"
                    dataSource={settings}
                    renderItem={item => (
                        <List.Item
                            actions={[
                                <Button
                                    type="primary"
                                    icon={<EditOutlined />}
                                    onClick={() => showDrawer(item.settingType, item.settingName)}
                                >
                                    编辑
                                </Button>
                            ]}
                        >
                            <List.Item.Meta
                                title={
                                    <Space>
                                        <Text strong>{item.settingName}</Text>
                                        <Tag color="blue">{item.settingType}</Tag>
                                    </Space>
                                }
                                description={
                                    <>
                                        <Paragraph type="secondary">
                                            最后修改时间: {new Date(item.lastModificationTime).toLocaleString()}
                                        </Paragraph>
                                    </>
                                }
                            />
                            <div style={{ maxWidth: '50%', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                                <Text ellipsis={{ tooltip: item.description }}>
                                    {item.description || '无内容'}
                                </Text>
                            </div>
                        </List.Item>
                    )}
                />
            </Skeleton>

            <Drawer
                title={currentSetting ? `编辑设置: ${currentSetting.settingName}` : '编辑设置'}
                width={800}
                onClose={closeDrawer}
                open={drawerVisible}
                bodyStyle={{ paddingBottom: 80 }}
                extra={
                    <Space>
                        <Button onClick={closeDrawer} icon={<CloseOutlined />}>取消</Button>
                        <Button
                            type="default"
                            onClick={handleFormatJSON}
                        >
                            格式化JSON
                        </Button>
                        <Button
                            type="primary"
                            onClick={() => form.submit()}
                            icon={<SaveOutlined />}
                            loading={loading}
                        >
                            保存
                        </Button>
                    </Space>
                }
            >
                {currentSetting && (
                    <>
                        <Paragraph>
                            <Text strong>设置类型:</Text> {currentSetting.settingType}
                        </Paragraph>
                        <Paragraph>
                            <Text strong>设置名称:</Text> {currentSetting.settingName}
                        </Paragraph>
                        {currentSetting.description && (
                            <Paragraph>
                                <Text strong>描述:</Text> {currentSetting.description}
                            </Paragraph>
                        )}
                        <Paragraph type="secondary">
                            最后修改时间: {new Date(currentSetting.lastModificationTime).toLocaleString()}
                        </Paragraph>

                        <Divider />

                        <Form
                            form={form}
                            layout="vertical"
                            onFinish={onFinish}
                        >
                            <Form.Item
                                name="settingContent"
                                label="设置内容"
                                rules={[
                                    { required: true, message: '请输入设置内容' },
                                    {
                                        validator: (_, value) => {
                                            if (!value) return Promise.resolve();
                                            try {
                                                JSON.parse(value);
                                                return Promise.resolve();
                                            } catch (e) {
                                                return Promise.reject('JSON格式无效，请检查');
                                            }
                                        }
                                    }
                                ]}
                            >
                                <TextArea
                                    rows={16}
                                    placeholder="请输入JSON格式的设置内容"
                                    style={{ fontFamily: 'monospace' }}
                                />
                            </Form.Item>
                        </Form>

                        <Divider />

                        <Paragraph type="secondary">
                            提示: 保存设置后可能需要重启应用或重新登录(刷新)才能生效。
                        </Paragraph>
                    </>
                )}
            </Drawer>
        </Card>
    );
};

export default SystemSetting;
