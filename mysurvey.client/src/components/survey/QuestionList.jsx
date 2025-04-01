import React, { useState, useEffect } from 'react';
import { Table, Button, Space, message, Drawer, Popconfirm, Typography, Tooltip } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, CopyOutlined } from '@ant-design/icons';
import { useParams } from 'react-router-dom';
import { surveyApi } from '../../services/api';
import QuestionEdit from './QuestionEdit';

const { Title } = Typography;

// 定义问题类型常量，与后端枚举保持一致
const QuestionType = {
    SingleChoice: 0,
    MultipleChoice: 1,
    TextInput: 2,
    Rating: 3,
    MatrixSingleChoice: 4,
    MatrixMultipleChoice: 5
};

const QuestionList = () => {
    const { id: surveyId } = useParams();
    const [questions, setQuestions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [drawerVisible, setDrawerVisible] = useState(false);
    const [editingQuestion, setEditingQuestion] = useState(null);
    const [isCreating, setIsCreating] = useState(false);
    const [survey, setSurvey] = useState(null);

    // 获取问卷信息和问题列表
    const fetchData = async () => {
        if (!surveyId) {
            console.error('surveyId is undefined in fetchData');
            message.error('问卷ID获取失败');
            return;
        }
        setLoading(true);
        try {
            console.log('Fetching survey with ID:', surveyId);
            const response = await surveyApi.getSurvey(surveyId);
            const surveyData = response.data;
            setSurvey(surveyData);
            setQuestions(surveyData.questions || []);
        } catch (error) {
            console.error('获取问题列表失败:', error);
            message.error('获取问题列表失败');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        console.log('Current surveyId:', surveyId);
        if (surveyId) {
            fetchData();
        } else {
            console.error('surveyId is undefined in useEffect');
            message.error('问卷ID获取失败');
        }
    }, [surveyId]);

    // 添加新问题
    const handleAddQuestion = () => {
        console.log('Adding question for surveyId:', surveyId);
        if (!surveyId) {
            console.error('surveyId is undefined in handleAddQuestion');
            message.error('问卷ID获取失败');
            return;
        }
        setIsCreating(true);
        setEditingQuestion({
            title: '',
            description: '',
            type: QuestionType.TextInput,
            isRequired: true,
            order: questions.length,
            options: []
        });
        setDrawerVisible(true);
    };

    // 编辑问题
    const handleEditQuestion = (question) => {
        if (!surveyId) {
            message.error('问卷ID不能为空');
            return;
        }
        if (!question?.id) {
            message.error('问题ID不能为空');
            return;
        }
        setIsCreating(false);
        setEditingQuestion(question);
        setDrawerVisible(true);
    };

    // 复制问题
    const handleCopyQuestion = async (question) => {
        try {
            const copiedQuestion = {
                ...question,
                title: `${question.title} (复制)`,
                order: questions.length,
                id: undefined
            };

            // 准备选项数据
            let processedOptions = [];

            // 根据问题类型处理选项
            if (copiedQuestion.type === QuestionType.SingleChoice ||
                copiedQuestion.type === QuestionType.MultipleChoice) {
                // 单选题和多选题需要处理选项
                const options = copiedQuestion.options || [];
                processedOptions = options.map(option => ({
                    content: option.content,
                    value: option.value || option.content,
                    order: option.order,
                    isDefault: option.isDefault || false,
                    isMatrixRow: option.isMatrixRow || false,
                    isMatrixColumn: option.isMatrixColumn || false
                }));
            } else if (copiedQuestion.type === QuestionType.Rating) {
                // 对于评分题，保留原始的最小值和最大值
                const options = copiedQuestion.options || [];
                let ratingMin = 1;
                let ratingMax = 5;

                if (options.length >= 2) {
                    ratingMin = parseInt(options[0].value) || 1;
                    ratingMax = parseInt(options[1].value) || 5;
                }

                // 创建两个选项来表示评分范围
                processedOptions = [
                    {
                        content: `${ratingMin}分`,
                        value: `${ratingMin}`,
                        order: 0,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    },
                    {
                        content: `${ratingMax}分`,
                        value: `${ratingMax}`,
                        order: 1,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    }
                ];
            }

            const requestData = {
                title: copiedQuestion.title,
                description: copiedQuestion.description,
                type: copiedQuestion.type,
                isRequired: copiedQuestion.isRequired,
                order: copiedQuestion.order,
                options: processedOptions
            };

            await surveyApi.addQuestion(surveyId, requestData);
            message.success('问题复制成功');
            fetchData();
        } catch (error) {
            message.error('复制问题失败');
            console.error(error);
        }
    };

    // 删除问题
    const handleDeleteQuestion = async (questionId) => {
        try {
            await surveyApi.deleteQuestion(surveyId, questionId);
            message.success('问题删除成功');
            fetchData();
        } catch (error) {
            message.error('删除问题失败');
            console.error(error);
        }
    };

    // 保存问题（新增或更新）
    const handleSaveQuestion = async (questionData) => {
        try {
            if (!surveyId) {
                message.error('问卷ID不能为空');
                return;
            }

            // 准备选项数据
            let processedOptions = [];

            // 根据问题类型处理选项
            if (questionData.type === QuestionType.SingleChoice ||
                questionData.type === QuestionType.MultipleChoice) {
                // 单选题和多选题需要处理选项
                processedOptions = (questionData.options || []).map(option => ({
                    content: option.content,
                    value: option.value || option.content,
                    order: option.order,
                    isDefault: option.isDefault || false,
                    isMatrixRow: option.isMatrixRow || false,
                    isMatrixColumn: option.isMatrixColumn || false
                }));
            } else if (questionData.type === QuestionType.Rating) {
                // 对于评分题，创建两个选项：一个表示最小值，一个表示最大值
                const ratingMin = questionData.ratingMin || 1; // 使用表单的ratingMin值或默认值1
                const ratingMax = questionData.ratingMax || 5; // 使用表单的ratingMax值或默认值5

                // 创建选项数组
                processedOptions = [
                    {
                        content: `${ratingMin}分`,
                        value: `${ratingMin}`,
                        order: 0,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    },
                    {
                        content: `${ratingMax}分`,
                        value: `${ratingMax}`,
                        order: 1,
                        isDefault: false,
                        isMatrixRow: false,
                        isMatrixColumn: false
                    }
                ];

                console.log('Rating question processed options:', processedOptions);
            }

            const requestData = {
                title: questionData.title,
                description: questionData.description,
                type: questionData.type,
                isRequired: questionData.isRequired,
                order: questionData.order,
                options: processedOptions,
                validationRuleType: questionData.validationRuleType,
                customValidationPattern: questionData.customValidationPattern,
                validationErrorMessage: questionData.validationErrorMessage
            };

            console.log('Saving question data:', requestData);

            if (isCreating) {
                await surveyApi.addQuestion(surveyId, requestData);
                message.success('问题添加成功');
            } else {
                if (!editingQuestion?.id) {
                    message.error('问题ID不能为空');
                    return;
                }
                await surveyApi.updateQuestion(surveyId, editingQuestion.id, requestData);
                message.success('问题更新成功');
            }

            setDrawerVisible(false);
            fetchData();
        } catch (error) {
            console.error('保存问题失败:', error);
            message.error(isCreating ? '添加问题失败' : '更新问题失败');
            if (error.response?.data?.message) {
                message.error(error.response.data.message);
            }
        }
    };

    // 表格列定义
    const columns = [
        {
            title: '序号',
            dataIndex: 'order',
            key: 'order',
            width: 80,
            sorter: (a, b) => a.order - b.order,
        },
        {
            title: '问题类型',
            dataIndex: 'type',
            key: 'type',
            width: 120,
            render: (type) => {
                const typeMap = {
                    [QuestionType.SingleChoice]: '单选题',
                    [QuestionType.MultipleChoice]: '多选题',
                    [QuestionType.TextInput]: '填空题',
                    [QuestionType.Rating]: '评分题',
                    [QuestionType.MatrixSingleChoice]: '矩阵单选题',
                    [QuestionType.MatrixMultipleChoice]: '矩阵多选题'
                };
                return typeMap[type] || '未知类型';
            },
        },
        {
            title: '问题标题',
            dataIndex: 'title',
            key: 'title',
            ellipsis: {
                showTitle: false,
            },
            render: (title) => (
                <Tooltip placement="topLeft" title={title}>
                    {title}
                </Tooltip>
            ),
        },
        {
            title: '必答',
            dataIndex: 'isRequired',
            key: 'isRequired',
            width: 80,
            render: (isRequired) => isRequired ? '是' : '否',
        },
        {
            title: '选项数',
            key: 'optionCount',
            width: 100,
            render: (_, record) => {
                // 对于评分题，显示评分范围而不是选项数
                if (record.type === QuestionType.Rating) {
                    const options = record.options || [];
                    if (options.length >= 2) {
                        return `${options[0].value}-${options[1].value}分`;
                    }
                    return '1-5分';
                }
                return record.options ? record.options.length : 0;
            },
        },
        {
            title: '操作',
            key: 'action',
            width: 200,
            render: (_, record) => (
                <Space>
                    <Button
                        type="text"
                        icon={<EditOutlined />}
                        onClick={() => handleEditQuestion(record)}
                    />
                    <Button
                        type="text"
                        icon={<CopyOutlined />}
                        onClick={() => handleCopyQuestion(record)}
                    />
                    <Popconfirm
                        title="确定要删除这个问题吗?"
                        onConfirm={() => handleDeleteQuestion(record.id)}
                        okText="确定"
                        cancelText="取消"
                    >
                        <Button
                            type="text"
                            danger
                            icon={<DeleteOutlined />}
                        />
                    </Popconfirm>
                </Space>
            ),
        },
    ];

    return (
        <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
                <Title level={4}>
                    {survey ? `问题管理: ${survey.title}` : '问题管理'}
                </Title>
                <Button
                    type="primary"
                    icon={<PlusOutlined />}
                    onClick={handleAddQuestion}
                >
                    添加问题
                </Button>
            </div>

            <Table
                columns={columns}
                dataSource={questions}
                rowKey="id"
                loading={loading}
                pagination={false}
            />

            <Drawer
                title={isCreating ? "添加问题" : "编辑问题"}
                placement="right"
                width={600}
                onClose={() => setDrawerVisible(false)}
                open={drawerVisible}
                destroyOnClose
            >
                {drawerVisible && (
                    <QuestionEdit
                        question={editingQuestion}
                        onSave={handleSaveQuestion}
                        onCancel={() => setDrawerVisible(false)}
                    />
                )}
            </Drawer>
        </div>
    );
};

export default QuestionList;
