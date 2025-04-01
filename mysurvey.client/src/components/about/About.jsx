import React from 'react';
import { Typography, Card, List, Space, Divider, Alert } from 'antd';
import { QuestionCircleOutlined, CheckCircleOutlined, WarningOutlined } from '@ant-design/icons';

const { Title, Paragraph, Text } = Typography;

const About = () => {
    // 功能列表
    const features = [
        {
            title: '问卷创建与管理',
            description: '创建、编辑、复制和删除问卷，灵活管理您的所有调查项目。'
        },
        {
            title: '多种问题类型',
            description: '支持单选题、多选题、填空题、评分题等多种问题类型，满足各类调查需求。'
        },
        {
            title: '问卷发布与分享',
            description: '一键发布问卷，生成专属链接供受访者填写，轻松分享到各个平台。'
        },
        {
            title: '数据收集与统计',
            description: '实时收集受访者的回答，自动生成统计图表，帮助您快速分析调查结果。'
        },
        {
            title: '用户认证与权限控制',
            description: '完善的用户认证系统，确保您的问卷数据安全可靠。'
        }
    ];

    // 使用注意事项
    const notes = [
        '请妥善保管您的账号密码，避免个人信息泄露。',
        '创建问卷时，请避免收集敏感个人信息，如身份证号、银行卡号等。',
        '问卷一旦发布后，修改问题可能会影响已收集的数据，请谨慎操作。',
        '定期导出并备份您的问卷数据，以防意外情况导致数据丢失。',
        '使用本平台创建的问卷内容需遵守相关法律法规，不得含有违法违规内容。'
    ];

    // 系统配置要求
    const requirements = [
        '推荐使用Chrome、Firefox、Edge等现代浏览器访问本系统。',
        '为获得最佳体验，请确保您的浏览器已启用JavaScript。',
        '移动端访问时，建议使用横屏模式获得更好的操作体验。'
    ];

    return (
        <div style={{ maxWidth: 800, margin: '0 auto' }}>
            <Typography>
                <Title level={2} style={{ textAlign: 'center' }}>关于MySurvey</Title>
                <Paragraph>
                    MySurvey是Biwen.QuickApi的示例项目!
                </Paragraph>
                <Paragraph>
                    旨在帮助帮助用户学习Biwen.QuickApi
                </Paragraph>

                <Divider orientation="left">主要功能</Divider>
                <List
                    itemLayout="horizontal"
                    dataSource={features}
                    renderItem={(item) => (
                        <List.Item>
                            <List.Item.Meta
                                avatar={<CheckCircleOutlined style={{ fontSize: '24px', color: '#52c41a' }} />}
                                title={<Text strong>{item.title}</Text>}
                                description={item.description}
                            />
                        </List.Item>
                    )}
                />

                <Divider orientation="left">使用注意事项</Divider>
                <Alert
                    message="安全与合规提示"
                    description={
                        <List
                            size="small"
                            dataSource={notes}
                            renderItem={(item) => (
                                <List.Item>
                                    <Space>
                                        <WarningOutlined style={{ color: '#faad14' }} />
                                        <Text>{item}</Text>
                                    </Space>
                                </List.Item>
                            )}
                        />
                    }
                    type="warning"
                    showIcon
                />

                <Divider orientation="left">系统要求</Divider>
                <Card>
                    <List
                        size="small"
                        dataSource={requirements}
                        renderItem={(item) => (
                            <List.Item>
                                <Space>
                                    <QuestionCircleOutlined style={{ color: '#1890ff' }} />
                                    <Text>{item}</Text>
                                </Space>
                            </List.Item>
                        )}
                    />
                </Card>

                <Divider />
                <Paragraph style={{ textAlign: 'center' }}>
                    <Text type="secondary">
                        本项目为Biwen.QuickApi的示例项目为MIT协议,因此你可以在任何地方使用!
                    </Text>
                </Paragraph>
            </Typography>
        </div>
    );
};

export default About;
